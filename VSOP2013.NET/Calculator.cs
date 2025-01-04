using System.Reflection;
using FastLZMA2Net;
using MemoryPack;

namespace VSOP2013
{
    public partial class Calculator
    {
        private List<PlanetTable> _vsop2013DATA;

        /// <summary>
        /// //Planetary frequency in longitude
        /// </summary>
        private readonly double[] _freqpla =
        {
            0.2608790314068555e5,
            0.1021328554743445e5,
            0.6283075850353215e4,
            0.3340612434145457e4,
            0.5296909615623250e3,
            0.2132990861084880e3,
            0.7478165903077800e2,
            0.3813297222612500e2,
            0.2533566020437000e2
        };

        private const double A1000 = 365250.0d;

        public Calculator()
        {
            //Import Planet Data
            var assembly = Assembly.GetExecutingAssembly();
            string datafilename = $"VSOP2013.NET.Resources.VSOP2013.BIN";
            using (MemoryStream recoveryStream = new MemoryStream())
            {
                using (Stream cs = assembly.GetManifestResourceStream(datafilename))
                {
                    using (DecompressStream ds = new DecompressStream(cs))
                    {
                        ds.CopyTo(recoveryStream);
                    }
                }

                _vsop2013DATA = MemoryPackSerializer.Deserialize<List<PlanetTable>>(recoveryStream.ToArray());
            }
        }

        public VSOPResult_ELL GetPlanetPosition(VSOPBody body, VSOPTime time)
        {
            double[] ELL = new double[6];
            ParallelLoopResult result = Parallel.For(0, 6, iv =>
            {
                ELL[iv] = GetVariable(body, iv, time);
            });
            VSOPResult_ELL Coordinate = new(body, time, ELL);
            return Coordinate;
        }

        public Task<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)
        {
            return Task.Run(() => GetPlanetPosition(body, time));
        }

        /// <summary>
        /// Calculate a specific variable
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="iv">0-5 : a l k h q p</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public double GetVariable(VSOPBody body, int iv, VSOPTime time)
        {
            return Calculate(_vsop2013DATA[(int)body].variables[iv], time.J2000);
        }
        public Task<double> GetVariableAsync(VSOPBody body, int iv, VSOPTime time)
        {
            return Task.Run(() => GetVariable(body, iv, time));
        }

        /// <summary>
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="JD2000"></param>
        /// <returns>Elliptic Elements</returns>
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
            double u, su, cu;
            double xl;
            Term[] terms;
            for (int it = 0; it < Table.PowerTables.Length; it++)
            {
                if (Table.PowerTables[it].Terms == null) continue;
                terms = Table.PowerTables[it].Terms;
                for (int n = 0; n < terms.Length; n++)
                {
                    u = terms[n].aa + terms[n].bb * tj;

                    su = Math.Sin(u);
                    cu = Math.Cos(u);

                    result += t[it] * (terms[n].ss * su + terms[n].cc * cu);
                }
            }
            if (Table.Variable == VSOPVariable.L)
            {
                xl = result + _freqpla[(int)Table.Body] * tj;
                xl = (xl % Math.Tau + Math.Tau) % Math.Tau;
                result = xl;
            }
            return result;
        }
    }
}
