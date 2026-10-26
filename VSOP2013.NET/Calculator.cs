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
    /// Full precision VSOP2013 calculator (double precision, matches the official reference values to 1e-10).
    /// </summary>
    /// <remarks>
    /// Instances are read-only after construction and safe to share across threads.
    /// For fast, low precision visualization use <see cref="CalculatorF"/>.
    /// </remarks>
    public partial class Calculator
    {
        private Dictionary<VSOPBody, PlanetTable> _vsop2013DATA;

        private const double A1000 = 365250.0d;

        /// <summary>
        /// Vector width actually used by this instance after hardware detection.
        /// </summary>
        public SimdMode Mode { get; }

        /// <summary>
        /// Truncation level rho: terms with sqrt(C^2 + S^2) &lt; rho are dropped at load time.
        /// </summary>
        public double Truncation { get; }

        private readonly int _lanes;

        /// <summary>
        /// Creates a full precision calculator with the widest accelerated vector width and no truncation.
        /// </summary>
        public Calculator() : this(SimdMode.Auto)
        {
        }

        /// <summary>
        /// Creates a calculator using the specified vector width.
        /// </summary>
        /// <param name="mode">Vector width; <see cref="SimdMode.Auto"/> picks the widest accelerated width.</param>
        /// <param name="truncation">
        /// Truncation level rho as defined by VSOP2013: terms with sqrt(C^2 + S^2) &lt; rho are removed
        /// (au for A, rad for L, unitless for K/H/Q/P). 0 (default) keeps the full series.
        /// </param>
        /// <exception cref="PlatformNotSupportedException">The requested width is not hardware accelerated.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="truncation"/> is negative or not finite.</exception>
        public Calculator(SimdMode mode, double truncation = 0d)
        {
            if (!double.IsFinite(truncation) || truncation < 0d)
            {
                throw new ArgumentOutOfRangeException(nameof(truncation), truncation, "Truncation must be a finite, non-negative value.");
            }
            Truncation = truncation;
            Mode = ResolveMode(mode);
            _lanes = Mode switch
            {
                SimdMode.Vector512 => Vector512<double>.Count,
                SimdMode.Vector256 => Vector256<double>.Count,
                SimdMode.Vector128 => Vector128<double>.Count,
                _ => 1,
            };

            //Import Planet Data
            string dataFilePath = Path.Combine(AppContext.BaseDirectory, "VSOP2013.BR");
            using (MemoryStream recoveryStream = new MemoryStream())
            {
                using (FileStream fs = new FileStream(dataFilePath, FileMode.Open, FileAccess.Read))
                {
                    using (BrotliStream bs = new BrotliStream(fs, CompressionMode.Decompress))
                    {
                        bs.CopyTo(recoveryStream);
                    }
                }

                _vsop2013DATA = MemoryPackSerializer.Deserialize<List<PlanetTable>>(recoveryStream.ToArray())
                    .ToDictionary(p => p.Body);
            }

            ExtractMeanMotion();
            BuildSoA();
        }

        /// <summary>
        /// Mean motion in longitude taken from the data (first term of L at power 1).
        /// </summary>
        private readonly Dictionary<VSOPBody, double> _meanMotion = new();

        // Low part of 2pi so that TauHi + TauLo carries ~107 bits.
        private const double TauLo = 2.4492935982947064e-16;

        /// <summary>
        /// Removes the mean-motion term from L so the periodic accumulator stays small,
        /// and keeps it for an exact-ish linear evaluation reduced modulo 2pi.
        /// </summary>
        private void ExtractMeanMotion()
        {
            foreach (PlanetTable planet in _vsop2013DATA.Values)
            {
                PowerTable[] powers = planet.variables[VSOPVariable.L].PowerTables;
                if (powers.Length < 2 || powers[1].Terms is not { Length: > 0 } terms)
                {
                    continue;
                }

                ref readonly Term first = ref terms[0];
                if (first.aa == 0d && first.bb == 0d && first.ss == 0d)
                {
                    _meanMotion[planet.Body] = first.cc;
                    powers[1].Terms = terms[1..];
                }
            }
        }

        /// <summary>
        /// Computes (freq * tj) mod 2pi without losing the low bits of the product.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double LinearModTau(double freq, double tj)
        {
            double p = freq * tj;
            double e = Math.FusedMultiplyAdd(freq, tj, -p);
            double k = Math.Round(p / Math.Tau);
            double r = Math.FusedMultiplyAdd(-k, Math.Tau, p);
            r = Math.FusedMultiplyAdd(-k, TauLo, r);
            return r + e;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double NormalizeL(VSOPBody body, double periodic, double tj)
        {
            double xl = periodic;
            if (_meanMotion.TryGetValue(body, out double freq))
            {
                xl += LinearModTau(freq, tj);
            }
            return (xl % Math.Tau + Math.Tau) % Math.Tau;
        }

        private static SimdMode ResolveMode(SimdMode mode)
        {
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
        }

        /// <summary>
        /// Split each PowerTable's Term[] into SoA arrays, dropping terms below the truncation level.
        /// Term[] is released afterwards; every path reads the SoA.
        /// </summary>
        private void BuildSoA()
        {
            double rho2 = Truncation * Truncation;
            foreach (PlanetTable planet in _vsop2013DATA.Values)
            {
                foreach (VariableTable variable in planet.variables.Values)
                {
                    foreach (PowerTable power in variable.PowerTables)
                    {
                        if (power.Terms == null)
                        {
                            continue;
                        }

                        Term[] terms = rho2 > 0d
                            ? Array.FindAll(power.Terms, t => t.cc * t.cc + t.ss * t.ss >= rho2)
                            : power.Terms;
                        power.Terms = null!;
                        if (terms.Length == 0)
                        {
                            continue;
                        }

                        int len = terms.Length;
                        // Zero padding (ss = cc = 0) contributes exactly 0, so the SIMD loop needs no scalar tail.
                        int padded = (len + _lanes - 1) / _lanes * _lanes;
                        double[] ss = new double[padded];
                        double[] cc = new double[padded];
                        double[] aa = new double[padded];
                        double[] bb = new double[padded];
                        for (int i = 0; i < len; i++)
                        {
                            ref readonly Term term = ref terms[i];
                            ss[i] = term.ss;
                            cc[i] = term.cc;
                            aa[i] = term.aa;
                            bb[i] = term.bb;
                        }
                        power.SS = ss;
                        power.CC = cc;
                        power.AA = aa;
                        power.BB = bb;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the six elliptic elements of a body; the elements are evaluated in parallel.
        /// </summary>
        /// <param name="body">Planet.</param>
        /// <param name="time">Time of evaluation.</param>
        /// <returns>Elliptic elements in the dynamical frame J2000.</returns>
        public VSOPResult_ELL GetPlanetPosition(VSOPBody body, VSOPTime time)
        {
            double[] ELL = new double[6];
            Parallel.ForEach(Enum.GetValues<VSOPVariable>(), variable =>
            {
                ELL[(int)variable - 1] = GetVariable(body, variable, time);
            });
            return new VSOPResult_ELL(body, time, ELL, ReferenceFrame.DynamicalJ2000);
        }

        /// <summary>
        /// Runs <see cref="GetPlanetPosition"/> on the thread pool.
        /// </summary>
        public ValueTask<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)
        {
            return new ValueTask<VSOPResult_ELL>(Task.Run(() => GetPlanetPosition(body, time)));
        }

        /// <summary>
        /// Calculates a single elliptic element.
        /// </summary>
        /// <param name="body">Planet.</param>
        /// <param name="variable">Element to compute (A/L/K/H/Q/P).</param>
        /// <param name="time">Time of evaluation.</param>
        /// <returns>The element value; L is normalized to [0, 2pi).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetVariable(VSOPBody body, VSOPVariable variable, VSOPTime time)
        {
            VariableTable table = _vsop2013DATA[body].variables[variable];
            double jd2000 = time.J2000;
            return Mode switch
            {
                SimdMode.Vector512 => CalculateV512(table, jd2000),
                SimdMode.Vector256 => CalculateV256(table, jd2000),
                SimdMode.Vector128 => CalculateV128(table, jd2000),
                _ => CalculateScalar(table, jd2000),
            };
        }

        /// <summary>
        /// Runs <see cref="GetVariable"/> on the thread pool.
        /// </summary>
        public ValueTask<double> GetVariableAsync(VSOPBody body, VSOPVariable variable, VSOPTime time)
        {
            return new ValueTask<double>(Task.Run(() => GetVariable(body, variable, time)));
        }

        /// <summary>
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="JD2000"></param>
        /// <returns>Elliptic Elements</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private double CalculateScalar(VariableTable Table, double JD2000)
        {
            //Thousand of Julian Years
            double tj = JD2000 / A1000;

            //Iteration on Time
            Span<double> t = stackalloc double[21];
            t[0] = 1.0d;
            t[1] = tj;
            for (int i = 2; i < 21; i++)
            {
                t[i] = t[1] * t[i - 1];
            }

            double result = 0d;
            for (int it = 0; it < Table.PowerTables.Length; it++)
            {
                PowerTable power = Table.PowerTables[it];
                if (power.AA == null)
                {
                    continue;
                }

                double[] ss = power.SS!;
                double[] cc = power.CC!;
                double[] aa = power.AA;
                double[] bb = power.BB!;
                for (int n = 0; n < aa.Length; n++)
                {
                    double u = Math.FusedMultiplyAdd(bb[n], tj, aa[n]);
                    double su = Math.Sin(u);
                    double cu = Math.Cos(u);
                    double term = Math.FusedMultiplyAdd(ss[n], su, cc[n] * cu);
                    result = Math.FusedMultiplyAdd(t[it], term, result);
                }
            }
            if (Table.Variable == VSOPVariable.L)
            {
                result = NormalizeL(Table.Body, result, tj);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private double CalculateV512(VariableTable Table, double JD2000)
        {
            double tj = JD2000 / A1000;
            Vector512<double> vtj = Vector512.Create(tj);
            PowerTable[] powerTables = Table.PowerTables;
            double tPow = 1d;
            double result = 0d;
            for (int it = 0; it < powerTables.Length; it++, tPow *= tj)
            {
                PowerTable power = powerTables[it];
                if (power.AA == null)
                {
                    continue;
                }

                int len = power.AA.Length;
                ref double ss = ref MemoryMarshal.GetArrayDataReference(power.SS!);
                ref double cc = ref MemoryMarshal.GetArrayDataReference(power.CC!);
                ref double aa = ref MemoryMarshal.GetArrayDataReference(power.AA);
                ref double bb = ref MemoryMarshal.GetArrayDataReference(power.BB!);
                Vector512<double> acc = Vector512<double>.Zero;
                for (int n = 0; n < len; n += Vector512<double>.Count)
                {
                    Vector512<double> u = Vector512.FusedMultiplyAdd(Vector512.LoadUnsafe(ref bb, (nuint)n), vtj, Vector512.LoadUnsafe(ref aa, (nuint)n));
                    (Vector512<double> su, Vector512<double> cu) = Vector512.SinCos(u);
                    acc += Vector512.FusedMultiplyAdd(Vector512.LoadUnsafe(ref ss, (nuint)n), su, Vector512.LoadUnsafe(ref cc, (nuint)n) * cu);
                }
                result = Math.FusedMultiplyAdd(tPow, Vector512.Sum(acc), result);
            }
            if (Table.Variable == VSOPVariable.L)
            {
                result = NormalizeL(Table.Body, result, tj);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private double CalculateV256(VariableTable Table, double JD2000)
        {
            double tj = JD2000 / A1000;
            Vector256<double> vtj = Vector256.Create(tj);
            PowerTable[] powerTables = Table.PowerTables;
            double tPow = 1d;
            double result = 0d;
            for (int it = 0; it < powerTables.Length; it++, tPow *= tj)
            {
                PowerTable power = powerTables[it];
                if (power.AA == null)
                {
                    continue;
                }

                int len = power.AA.Length;
                ref double ss = ref MemoryMarshal.GetArrayDataReference(power.SS!);
                ref double cc = ref MemoryMarshal.GetArrayDataReference(power.CC!);
                ref double aa = ref MemoryMarshal.GetArrayDataReference(power.AA);
                ref double bb = ref MemoryMarshal.GetArrayDataReference(power.BB!);
                Vector256<double> acc = Vector256<double>.Zero;
                for (int n = 0; n < len; n += Vector256<double>.Count)
                {
                    Vector256<double> u = Vector256.FusedMultiplyAdd(Vector256.LoadUnsafe(ref bb, (nuint)n), vtj, Vector256.LoadUnsafe(ref aa, (nuint)n));
                    (Vector256<double> su, Vector256<double> cu) = Vector256.SinCos(u);
                    acc += Vector256.FusedMultiplyAdd(Vector256.LoadUnsafe(ref ss, (nuint)n), su, Vector256.LoadUnsafe(ref cc, (nuint)n) * cu);
                }
                result = Math.FusedMultiplyAdd(tPow, Vector256.Sum(acc), result);
            }
            if (Table.Variable == VSOPVariable.L)
            {
                result = NormalizeL(Table.Body, result, tj);
            }
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private double CalculateV128(VariableTable Table, double JD2000)
        {
            double tj = JD2000 / A1000;
            Vector128<double> vtj = Vector128.Create(tj);
            PowerTable[] powerTables = Table.PowerTables;
            double tPow = 1d;
            double result = 0d;
            for (int it = 0; it < powerTables.Length; it++, tPow *= tj)
            {
                PowerTable power = powerTables[it];
                if (power.AA == null)
                {
                    continue;
                }

                int len = power.AA.Length;
                ref double ss = ref MemoryMarshal.GetArrayDataReference(power.SS!);
                ref double cc = ref MemoryMarshal.GetArrayDataReference(power.CC!);
                ref double aa = ref MemoryMarshal.GetArrayDataReference(power.AA);
                ref double bb = ref MemoryMarshal.GetArrayDataReference(power.BB!);
                Vector128<double> acc = Vector128<double>.Zero;
                for (int n = 0; n < len; n += Vector128<double>.Count)
                {
                    Vector128<double> u = Vector128.FusedMultiplyAdd(Vector128.LoadUnsafe(ref bb, (nuint)n), vtj, Vector128.LoadUnsafe(ref aa, (nuint)n));
                    (Vector128<double> su, Vector128<double> cu) = Vector128.SinCos(u);
                    acc += Vector128.FusedMultiplyAdd(Vector128.LoadUnsafe(ref ss, (nuint)n), su, Vector128.LoadUnsafe(ref cc, (nuint)n) * cu);
                }
                result = Math.FusedMultiplyAdd(tPow, Vector128.Sum(acc), result);
            }
            if (Table.Variable == VSOPVariable.L)
            {
                result = NormalizeL(Table.Body, result, tj);
            }
            return result;
        }
    }
}
