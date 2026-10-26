// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

using MemoryPack;

namespace VSOP2013
{
    /// <summary>
    /// Single precision approximation of VSOP2013 intended for visualization.
    /// Every step (time, coefficients, phase, sin/cos, accumulation) runs in float.
    /// Typical error over +-1000 years: L up to ~3e-3 rad (Mercury), A up to ~1e-4 au (Pluto).
    /// Use <see cref="Calculator"/> when accuracy matters.
    /// </summary>
    public class CalculatorF
    {
        private const float A1000 = 365250f;

        /// <summary>
        /// One power segment stored as SoA float arrays, zero padded to the selected vector width.
        /// </summary>
        private sealed class PowerSoA(float[] ss, float[] cc, float[] aa, float[] bb)
        {
            public readonly float[] SS = ss;
            public readonly float[] CC = cc;
            public readonly float[] AA = aa;
            public readonly float[] BB = bb;
        }

        // [body, variable] -> power segments (index = power of t). Null entries are empty powers.
        private readonly PowerSoA?[][][] _tables;

        private readonly int _lanes;

        /// <summary>
        /// Vector width actually used by this instance after hardware detection.
        /// </summary>
        public SimdMode Mode { get; }

        /// <summary>
        /// Default truncation level used by <see cref="CalculatorF"/>.
        /// </summary>
        public const double DefaultTruncation = 1e-8;

        /// <summary>
        /// Truncation level rho: terms with sqrt(C^2 + S^2) &lt; rho are dropped at load time.
        /// </summary>
        public double Truncation { get; }

        public CalculatorF() : this(SimdMode.Auto)
        {
        }

        /// <summary>
        /// Creates a single precision calculator using the specified vector width.
        /// </summary>
        /// <param name="mode">Vector width; <see cref="SimdMode.Auto"/> picks the widest accelerated width.</param>
        /// <param name="truncation">
        /// Truncation level rho as defined by VSOP2013: terms with sqrt(C^2 + S^2) &lt; rho are removed
        /// (au for A, rad for L, unitless for K/H/Q/P). 0 keeps the full series.
        /// The official tables assume |t| &lt;= 1 millennium; beyond that, high powers of t amplify the truncation error.
        /// </param>
        /// <exception cref="PlatformNotSupportedException">The requested width is not hardware accelerated.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="truncation"/> is negative or not finite.</exception>
        public CalculatorF(SimdMode mode, double truncation = DefaultTruncation)
        {
            if (!double.IsFinite(truncation) || truncation < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(truncation), truncation, "Truncation must be a finite, non-negative value.");
            }
            Truncation = truncation;
            Mode = ResolveMode(mode);
            _lanes = Mode switch
            {
#if NET9_0_OR_GREATER
                SimdMode.Vector512 => Vector512<float>.Count,
                SimdMode.Vector256 => Vector256<float>.Count,
                SimdMode.Vector128 => Vector128<float>.Count,
#endif
                _ => 1,
            };

            string dataFilePath = Path.Combine(AppContext.BaseDirectory, "VSOP2013.BR");
            List<PlanetTable> planets;
            using (MemoryStream recoveryStream = new MemoryStream())
            {
                using (FileStream fs = new FileStream(dataFilePath, FileMode.Open, FileAccess.Read))
                using (BrotliStream bs = new BrotliStream(fs, CompressionMode.Decompress))
                {
                    bs.CopyTo(recoveryStream);
                }
                planets = MemoryPackSerializer.Deserialize<List<PlanetTable>>(recoveryStream.ToArray())!;
            }

            // Term[] is only needed while building the float SoA; it is dropped afterwards.
            int bodyCount = Enum.GetValues<VSOPBody>().Max(b => (int)b) + 1;
            int variableCount = Enum.GetValues<VSOPVariable>().Max(v => (int)v) + 1;
            _tables = new PowerSoA?[bodyCount][][];
            foreach (PlanetTable planet in planets)
            {
                PowerSoA?[][] variables = new PowerSoA?[variableCount][];
                foreach (VariableTable variable in planet.variables.Values)
                {
                    variables[(int)variable.Variable] = BuildSoA(variable.PowerTables);
                }
                _tables[(int)planet.Body] = variables;
            }
        }

        private PowerSoA?[] BuildSoA(PowerTable[] powers)
        {
            double rho2 = Truncation * Truncation;
            PowerSoA?[] result = new PowerSoA?[powers.Length];
            for (int it = 0; it < powers.Length; it++)
            {
                Term[]? source = powers[it].Terms;
                if (source is not { Length: > 0 })
                {
                    continue;
                }

                Term[] terms = rho2 > 0d
                    ? Array.FindAll(source, t => t.cc * t.cc + t.ss * t.ss >= rho2)
                    : source;
                if (terms.Length == 0)
                {
                    continue;
                }

                int len = terms.Length;
                // Zero padding (ss = cc = 0) contributes exactly 0, so the loops need no tail.
                int padded = (len + _lanes - 1) / _lanes * _lanes;
                float[] ss = new float[padded];
                float[] cc = new float[padded];
                float[] aa = new float[padded];
                float[] bb = new float[padded];
                for (int i = 0; i < len; i++)
                {
                    ss[i] = (float)terms[i].ss;
                    cc[i] = (float)terms[i].cc;
                    aa[i] = (float)terms[i].aa;
                    bb[i] = (float)terms[i].bb;
                }
                result[it] = new PowerSoA(ss, cc, aa, bb);
            }
            return result;
        }

        private static SimdMode ResolveMode(SimdMode mode)
        {
#if NET9_0_OR_GREATER
            return mode switch
            {
                SimdMode.Auto => Vector512.IsHardwareAccelerated ? SimdMode.Vector512
                    : Vector256.IsHardwareAccelerated ? SimdMode.Vector256
                    : Vector128.IsHardwareAccelerated ? SimdMode.Vector128
                    : SimdMode.Scalar,
                SimdMode.Scalar => SimdMode.Scalar,
                SimdMode.Vector128 when Vector128.IsHardwareAccelerated => mode,
                SimdMode.Vector256 when Vector256.IsHardwareAccelerated => mode,
                SimdMode.Vector512 when Vector512.IsHardwareAccelerated => mode,
                SimdMode.Vector128 or SimdMode.Vector256 or SimdMode.Vector512 =>
                    throw new PlatformNotSupportedException($"{mode} is not hardware accelerated on this machine."),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
            };
#else
            return mode switch
            {
                SimdMode.Auto or SimdMode.Scalar => SimdMode.Scalar,
                SimdMode.Vector128 or SimdMode.Vector256 or SimdMode.Vector512 =>
                    throw new PlatformNotSupportedException($"{mode} requires .NET 9 or later."),
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null),
            };
#endif
        }

        public VSOPResult_ELL GetPlanetPosition(VSOPBody body, VSOPTime time)
        {
            double[] ELL = new double[6];
            for (int i = 0; i < ELL.Length; i++)
            {
                ELL[i] = GetVariable(body, (VSOPVariable)(i + 1), time);
            }
            return new VSOPResult_ELL(body, time, ELL, ReferenceFrame.DynamicalJ2000);
        }

        public ValueTask<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)
        {
            return new ValueTask<VSOPResult_ELL>(Task.Run(() => GetPlanetPosition(body, time)));
        }

        /// <summary>
        /// Calculate a specific variable in single precision.
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="variable">VSOPVariable.A/L/K/H/Q/P</param>
        /// <param name="time"></param>
        public float GetVariable(VSOPBody body, VSOPVariable variable, VSOPTime time)
        {
            PowerSoA?[] powers = _tables[(int)body]?[(int)variable]
                ?? throw new ArgumentOutOfRangeException(nameof(body), body, null);
            float tj = (float)time.J2000 / A1000;
            float result = Mode switch
            {
#if NET9_0_OR_GREATER
                SimdMode.Vector512 => Calculate_Vector512(powers, tj),
                SimdMode.Vector256 => Calculate_Vector256(powers, tj),
                SimdMode.Vector128 => Calculate_Vector128(powers, tj),
#endif
                _ => Calculate(powers, tj),
            };
            if (variable == VSOPVariable.L)
            {
                result = (result % MathF.Tau + MathF.Tau) % MathF.Tau;
            }
            return result;
        }

        public ValueTask<float> GetVariableAsync(VSOPBody body, VSOPVariable variable, VSOPTime time)
        {
            return new ValueTask<float>(Task.Run(() => GetVariable(body, variable, time)));
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float Calculate(PowerSoA?[] powers, float tj)
        {
            float tPow = 1f;
            float result = 0f;
            for (int it = 0; it < powers.Length; it++, tPow *= tj)
            {
                PowerSoA? power = powers[it];
                if (power == null)
                {
                    continue;
                }

                float[] ss = power.SS, cc = power.CC, aa = power.AA, bb = power.BB;
                float sum = 0f;
                for (int n = 0; n < aa.Length; n++)
                {
                    (float su, float cu) = MathF.SinCos(MathF.FusedMultiplyAdd(bb[n], tj, aa[n]));
                    sum += MathF.FusedMultiplyAdd(ss[n], su, cc[n] * cu);
                }
                result = MathF.FusedMultiplyAdd(tPow, sum, result);
            }
            return result;
        }

#if NET9_0_OR_GREATER
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float Calculate_Vector512(PowerSoA?[] powers, float tj)
        {
            Vector512<float> vtj = Vector512.Create(tj);
            float tPow = 1f;
            float result = 0f;
            for (int it = 0; it < powers.Length; it++, tPow *= tj)
            {
                PowerSoA? power = powers[it];
                if (power == null)
                {
                    continue;
                }

                int len = power.AA.Length;
                ref float ss = ref MemoryMarshal.GetArrayDataReference(power.SS);
                ref float cc = ref MemoryMarshal.GetArrayDataReference(power.CC);
                ref float aa = ref MemoryMarshal.GetArrayDataReference(power.AA);
                ref float bb = ref MemoryMarshal.GetArrayDataReference(power.BB);
                Vector512<float> acc = Vector512<float>.Zero;
                for (int n = 0; n < len; n += Vector512<float>.Count)
                {
                    Vector512<float> u = Vector512.FusedMultiplyAdd(Vector512.LoadUnsafe(ref bb, (nuint)n), vtj, Vector512.LoadUnsafe(ref aa, (nuint)n));
                    (Vector512<float> su, Vector512<float> cu) = Vector512.SinCos(u);
                    acc += Vector512.FusedMultiplyAdd(Vector512.LoadUnsafe(ref ss, (nuint)n), su, Vector512.LoadUnsafe(ref cc, (nuint)n) * cu);
                }
                result = MathF.FusedMultiplyAdd(tPow, Vector512.Sum(acc), result);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float Calculate_Vector256(PowerSoA?[] powers, float tj)
        {
            Vector256<float> vtj = Vector256.Create(tj);
            float tPow = 1f;
            float result = 0f;
            for (int it = 0; it < powers.Length; it++, tPow *= tj)
            {
                PowerSoA? power = powers[it];
                if (power == null)
                {
                    continue;
                }

                int len = power.AA.Length;
                ref float ss = ref MemoryMarshal.GetArrayDataReference(power.SS);
                ref float cc = ref MemoryMarshal.GetArrayDataReference(power.CC);
                ref float aa = ref MemoryMarshal.GetArrayDataReference(power.AA);
                ref float bb = ref MemoryMarshal.GetArrayDataReference(power.BB);
                Vector256<float> acc = Vector256<float>.Zero;
                for (int n = 0; n < len; n += Vector256<float>.Count)
                {
                    Vector256<float> u = Vector256.FusedMultiplyAdd(Vector256.LoadUnsafe(ref bb, (nuint)n), vtj, Vector256.LoadUnsafe(ref aa, (nuint)n));
                    (Vector256<float> su, Vector256<float> cu) = Vector256.SinCos(u);
                    acc += Vector256.FusedMultiplyAdd(Vector256.LoadUnsafe(ref ss, (nuint)n), su, Vector256.LoadUnsafe(ref cc, (nuint)n) * cu);
                }
                result = MathF.FusedMultiplyAdd(tPow, Vector256.Sum(acc), result);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static float Calculate_Vector128(PowerSoA?[] powers, float tj)
        {
            Vector128<float> vtj = Vector128.Create(tj);
            float tPow = 1f;
            float result = 0f;
            for (int it = 0; it < powers.Length; it++, tPow *= tj)
            {
                PowerSoA? power = powers[it];
                if (power == null)
                {
                    continue;
                }

                int len = power.AA.Length;
                ref float ss = ref MemoryMarshal.GetArrayDataReference(power.SS);
                ref float cc = ref MemoryMarshal.GetArrayDataReference(power.CC);
                ref float aa = ref MemoryMarshal.GetArrayDataReference(power.AA);
                ref float bb = ref MemoryMarshal.GetArrayDataReference(power.BB);
                Vector128<float> acc = Vector128<float>.Zero;
                for (int n = 0; n < len; n += Vector128<float>.Count)
                {
                    Vector128<float> u = Vector128.FusedMultiplyAdd(Vector128.LoadUnsafe(ref bb, (nuint)n), vtj, Vector128.LoadUnsafe(ref aa, (nuint)n));
                    (Vector128<float> su, Vector128<float> cu) = Vector128.SinCos(u);
                    acc += Vector128.FusedMultiplyAdd(Vector128.LoadUnsafe(ref ss, (nuint)n), su, Vector128.LoadUnsafe(ref cc, (nuint)n) * cu);
                }
                result = MathF.FusedMultiplyAdd(tPow, Vector128.Sum(acc), result);
            }
            return result;
        }
#endif
    }
}
