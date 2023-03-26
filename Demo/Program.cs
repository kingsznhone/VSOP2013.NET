using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using VSOP2013;
using VSOP2013.VSOPResult;

namespace Demo
{
    class Program
    {
        static Calculator vsop;
        static VSOPTime vTime;
        static void Main(string[] args)
        {
            DateTime Tinput;

            vsop = new Calculator();
            
            Tinput = DateTime.Now.ToUniversalTime();

            //Console.WriteLine("Parse UTC string that conforms to ISO 8601:  2018-08-18T07:22:16.0000000Z");
            
            //Parse Time
            //while (true)
            //{
            //    Console.Write("Input Time As UTC:");
            //    //string inputT = Console.ReadLine();
            //    string inputT = "2000-01-01T12:00:00.0000000Z";
            //    CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            //    DateTimeStyles style = DateTimeStyles.AdjustToUniversal;
            //    if (DateTime.TryParse(inputT, culture, style, out Tinput)) break;
            //    else Console.WriteLine("Invalid Entry...");
            //}
            //Tinput = VSOPTime.TDBtoTT(Tinput);
            //Tinput = VSOPTime.TTtoTAI(Tinput);
            //Tinput = VSOPTime.TAItoUTC(Tinput);

            Console.WriteLine(Tinput.ToString());

            //Convert UTC to TDB (Barycentric Dynamical Time)
            vTime = new VSOPTime(Tinput);

            Console.WriteLine();

            Console.WriteLine("Press Enter To Start Substitution...");
            Console.ReadLine();

            DynamicalELL[] ResultAll = vsop.CalcAllPlanet(vTime);

            foreach(DynamicalELL result in ResultAll)
            {
                FormattedPrint(result, vTime);
            }

            Console.WriteLine("Press Enter to Start PerformanceTest.");
            Console.ReadLine();

            
            PerformanceTest(1000);
            Console.ReadLine();
        }

        public static void FormattedPrint(DynamicalELL Result, VSOPTime vtime)
        {
            Console.WriteLine("===============================================================");
            Console.WriteLine("PLANETARY EPHEMERIS VSOP2013");
            Console.WriteLine("===============================================================");

            Console.WriteLine(Enum.GetName(typeof(VSOPBody), Result.Body) + " at UTC:" + vtime.UTC.ToString());
            Console.WriteLine();
            Console.WriteLine("            Elliptic Elements - Dynamical Frame J2000");
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "semi-major axis (au)", (Result).A));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "mean longitude (rd)", (Result).L));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "k = e*cos(pi) (rd)", (Result).K));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "h = e*sin(pi) (rd)", (Result).H));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "q = sin(i/2)*cos(omega) (rd)", (Result).Q));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "p = sin(i/2)*sin(omega) (rd)", (Result).P));
            Console.WriteLine();
            Console.WriteLine("            Ecliptic Heliocentric Coordinates - Dynamical Frame J2000");
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Positions  X (au)", ((DynamicalXYZ)Result).X));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Positions  Y (au)", ((DynamicalXYZ)Result).Y));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Positions  Z (au)", ((DynamicalXYZ)Result).Z));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Velocities X'(au/d)", ((DynamicalXYZ)Result).dX));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Velocities Y'(au/d)", ((DynamicalXYZ)Result).dY));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Velocities Z'(au/d)", ((DynamicalXYZ)Result).dZ));
            Console.WriteLine();
            Console.WriteLine("            Equatorial Heliocentric Coordinates - ICRS Frame J2000");
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Positions  X (au)", ((ICRSXYZ)Result).X));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Positions  Y (au)", ((ICRSXYZ)Result).Y));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Positions  Z (au)", ((ICRSXYZ)Result).Z));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Velocities X'(au/d)", ((ICRSXYZ)Result).dX));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Velocities Y'(au/d)", ((ICRSXYZ)Result).dY));
            Console.WriteLine(String.Format("{0,-30} : {1,-30}", "Velocities Z'(au/d)", ((ICRSXYZ)Result).dZ));
            Console.WriteLine();
        }


        public static void PerformanceTest(int cycle)
        {
            Console.WriteLine();
            Console.WriteLine("=====================Start Performance Test=====================");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int completedCycle = 0;
            for (int i = 0; i < cycle; i++)
            {
                DynamicalELL[] ResultAll = vsop.CalcAllPlanet(vTime);
                completedCycle++;
                if (completedCycle % 100 == 0 && completedCycle<cycle)
                {
                    Console.WriteLine($"Cycle: {completedCycle}/{cycle,-10}  {sw.Elapsed.TotalMilliseconds,10} ms");
                }
            }
            sw.Stop();
            Console.WriteLine($"Total: {completedCycle}/{cycle,-10}  {sw.Elapsed.TotalMilliseconds,10} ms");
        }
    }
}
