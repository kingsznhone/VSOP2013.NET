using System.Diagnostics;

namespace VSOP2013
{

    public class Calculator
    {
        public readonly List<PlanetTable> VSOP2013DATA;

        public TimeSpan TimeUsed;

        Stopwatch sw;

        /// <summary>
        /// //Planetary frequency in longitude
        /// </summary>
        readonly double[] freqpla =
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

        const double dpi = 2 * Math.PI;
        const double a1000 = 365250.0d;

        public Calculator()
        {
            //Import Planet Data
            sw = new Stopwatch();
            TimeUsed = new TimeSpan(0);

            sw.Start();
            VSOP2013DATA = DataReader.ReadData();
            sw.Stop();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            double ticks = sw.ElapsedTicks;
            double Freq = Stopwatch.Frequency;
            double milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Data Load OK...Elapsed milliseconds: {milliseconds} ms");

        }

        public VSOPResult[] CalcAllPlanet(VSOPTime time)
        {
            VSOPResult[] vsopresult = new VSOPResult[9];

            ParallelLoopResult result = Parallel.For(0, 9, ip =>
            {
                vsopresult[ip] = CalcPlanet((VSOPBody)ip, time);
            });
            return vsopresult;
        }

        public VSOPResult CalcPlanet(VSOPBody body, VSOPTime time)
        {

            double[] ELL = new double[6];
            ParallelLoopResult result = Parallel.For(0, 6, iv =>
            {
                ELL[iv] = CalcIV(body, iv, time);
            });
            VSOPResult Coordinate = new VSOPResult(body, time, ELL);
            return Coordinate;
        }

        /// <summary>
        /// Calculate a specific variable
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="iv">0-5 : a l k h q p</param>
        /// <param name="time"></param>
        /// <returns></returns>
        public double CalcIV(VSOPBody body, int iv, VSOPTime time)
        {
            return CalcIV(VSOP2013DATA[(int)body].variables[iv], VSOPTime.ToJulianDate2000(time.TDB));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vTable"></param>
        /// <param name="JD2000"></param>
        /// <returns>Elliptic Elements</returns>
        private double CalcIV(VariableTable vTable, double JD2000)
        {

            double tj = JD2000 / a1000;
            //Iteration on Time 
            double[] t = new double[21];
            t[0] = 1.0d;
            t[1] = tj;
            for (int i = 2; i < 21; i++)
            {
                t[i] = t[1] * t[i - 1];
            }

            double result = 0d;
            double arg;
            double sarg;
            double carg;
            double xl;
            double tit;
            for (int it = 0; it < vTable.PowerTables.Length; it++)
            {
                tit = t[it];
                if (vTable.PowerTables[it].Terms == null) continue;
                for (int n = 0; n < vTable.PowerTables[it].Terms.Length; n++)
                {
                    arg = vTable.PowerTables[it].Terms[n].aa + vTable.PowerTables[it].Terms[n].bb * tj;
                    (sarg, carg) = Math.SinCos(arg);
                    result += tit * (vTable.PowerTables[it].Terms[n].ss * sarg + vTable.PowerTables[it].Terms[n].cc * carg);
                }
            }
            if (vTable.iv == 1)
            {
                xl = result + freqpla[(int)vTable.Body] * tj;
                xl = xl % dpi;
                if (xl < 0) xl = xl + dpi;
                result = xl;
            }
            return result;
        }
    }
}