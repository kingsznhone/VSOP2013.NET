using System.IO.Compression;
using System.Runtime.CompilerServices;

using MemoryPack;

namespace VSOP2013
{
    public partial class Calculator
    {
        private Dictionary<VSOPBody, PlanetTable> _vsop2013DATA;

        /// <summary>
        /// //Planetary frequency in longitude
        /// </summary>
        private static readonly Dictionary<VSOPBody, double> _freqpla = new()
        {
            { VSOPBody.MERCURY, 0.2608790314068555e5d },
            { VSOPBody.VENUS,   0.1021328554743445e5d },
            { VSOPBody.EMB,     0.6283075850353215e4d },
            { VSOPBody.MARS,    0.3340612434145457e4d },
            { VSOPBody.JUPITER, 0.5296909615623250e3d },
            { VSOPBody.SATURN,  0.2132990861084880e3d },
            { VSOPBody.URANUS,  0.7478165903077800e2d },
            { VSOPBody.NEPTUNE, 0.3813297222612500e2d },
            { VSOPBody.PLUTO,   0.2533566020437000e2d },
        };

        private const double A1000 = 365250.0d;

        public Calculator()
        {
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
        }

        public VSOPResult_ELL GetPlanetPosition(VSOPBody body, VSOPTime time)
        {
            double[] ELL = new double[6];
            Parallel.ForEach(Enum.GetValues<VSOPVariable>(), variable =>
            {
                ELL[(int)variable - 1] = GetVariable(body, variable, time);
            });
            return new VSOPResult_ELL(body, time, ELL, ReferenceFrame.DynamicalJ2000);
        }

        public ValueTask<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)
        {
            return new ValueTask<VSOPResult_ELL>(Task.Run(() => GetPlanetPosition(body, time)));
        }

        /// <summary>
        /// Calculate a specific variable
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="variable">VSOPVariable.A/L/K/H/Q/P</param>
        /// <param name="time"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetVariable(VSOPBody body, VSOPVariable variable, VSOPTime time)
        {
            return Calculate(_vsop2013DATA[body].variables[variable], time.J2000);
        }

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
        private double Calculate(VariableTable Table, double JD2000)
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
                if (Table.PowerTables[it].Terms == null) continue;
                ref readonly Term[] terms = ref Table.PowerTables[it].Terms;
                for (int n = 0; n < terms.Length; n++)
                {
                    double u = terms[n].aa + terms[n].bb * tj;
                    double su = Math.Sin(u);
                    double cu = Math.Cos(u);
                    result += t[it] * (terms[n].ss * su + terms[n].cc * cu);
                }
            }
            if (Table.Variable == VSOPVariable.L)
            {
                double xl;
                xl = result + _freqpla[Table.Body] * tj;
                xl = (xl % Math.Tau + Math.Tau) % Math.Tau;
                result = xl;
            }
            return result;
        }
    }
}
