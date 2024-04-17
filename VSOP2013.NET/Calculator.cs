using MessagePack;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.InteropServices;

namespace VSOP2013
{
    public class Calculator
    {

        [DllImport("Resources/NativeAccelerator.dll")]
        public static extern double StartIteration(Term[] terms,int length, double tj, double tit);

        public List<PlanetTable> VSOP2013DATA;

        /// <summary>
        /// //Planetary frequency in longitude
        /// </summary>
        private readonly double[] freqpla =
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

        private const double a1000 = 365250.0d;

        public Calculator()
        {
            //Import Planet Data
            var assembly = Assembly.GetExecutingAssembly();
            //var names = assembly.GetManifestResourceNames();
            VSOP2013DATA = new List<PlanetTable>(9);
            ParallelLoopResult result = Parallel.For(0, 9, ip =>
            {
                string datafilename = $"VSOP2013.NET.Resources.VSOP2013_{(VSOPBody)(ip)}.BIN";
                using Stream s = assembly.GetManifestResourceStream(datafilename);
                using BrotliStream bs = new(s, CompressionMode.Decompress);
                var data = MessagePackSerializer.Deserialize<PlanetTable>(bs);
                VSOP2013DATA.Add(data);
            });

            VSOP2013DATA = VSOP2013DATA.OrderBy(x => x.body).ToList();

            GC.Collect();
            GC.WaitForPendingFinalizers();
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
        public VSOPResult_ELL GetPlanetPosition_Native(VSOPBody body, VSOPTime time)
        {
            double[] ELL = new double[6];
            ParallelLoopResult result = Parallel.For(0, 6, iv =>
            {
                ELL[iv] = GetVariable_Native(body, iv, time);
            });
            VSOPResult_ELL Coordinate = new(body, time, ELL);
            return Coordinate;
        }

        public async Task<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)
        {
            return await Task.Run(() => GetPlanetPosition(body, time));
        }
        public async Task<VSOPResult_ELL> GetPlanetPositionAsync_Native(VSOPBody body, VSOPTime time)
        {
            return await Task.Run(() => GetPlanetPosition_Native(body, time));
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
            return Calculate(VSOP2013DATA[(int)body].variables[iv], time.J2000);
        }
        public double GetVariable_Native(VSOPBody body, int iv, VSOPTime time)
        {
            return Calculate_Native(VSOP2013DATA[(int)body].variables[iv], time.J2000);
        }

        public async Task<double> GetVariableAsync(VSOPBody body, int iv, VSOPTime time)
        {
            return await Task.Run(() => GetVariable(body, iv, time));
        }
        public async Task<double> GetVariableAsync_Native(VSOPBody body, int iv, VSOPTime time)
        {
            return await Task.Run(() => GetVariable_Native(body, iv, time));
        }

        /// <summary>
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="JD2000"></param>
        /// <returns>Elliptic Elements</returns>
        private double Calculate(VariableTable Table, double JD2000)
        {
            //Thousand of Julian Years
            double tj = JD2000 / a1000;

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
#if NET8_0
                    su = Math.Sin(u);
                    cu = Math.Cos(u);
#else
                    (su, cu) = Math.SinCos(u);
#endif
                    result += t[it] * (terms[n].ss * su + terms[n].cc * cu);
                }
            }
            if (Table.Variable == VSOPVariable.A)
            {
                xl = result + freqpla[(int)Table.Body] * tj;
                xl = (xl % Math.Tau + Math.Tau) % Math.Tau;
                result = xl;
            }
            return result;
        }

        private unsafe double Calculate_Native(VariableTable Table, double JD2000)
        {
            //Thousand of Julian Years
            double tj = JD2000 / a1000;

            //Iteration on Time
            Span<double> t = stackalloc double[21];
            t[0] = 1.0d;
            t[1] = tj;
            for (int i = 2; i < 21; i++)
            {
                t[i] = t[1] * t[i - 1];
            }

            double result = 0d;
            double xl;
            for (int it = 0; it < Table.PowerTables.Length; it++)
            {
                if (Table.PowerTables[it].Terms == null) continue;
                Term[] terms = Table.PowerTables[it].Terms;
                result += StartIteration(Table.PowerTables[it].Terms, terms.Length, tj, t[it]);
            }
            if (Table.Variable == (VSOPVariable)1)
            {
                xl = result + freqpla[(int)Table.Body] * tj;
                xl = (xl % Math.Tau + Math.Tau) % Math.Tau;
                result = xl;
            }
            return result;
        }

    }
}