using System;
using System.Globalization;
using VSOP2013;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;

namespace Demo
{
    internal class Program
    {
        private static Calculator vsop;

        private static void Main(string[] args)
        {
            vsop = new Calculator();
            //Console.WriteLine("Parse UTC string that conforms to ISO 8601:  2018-08-18T07:22:16.0000000Z");

            //Parse Time
            //while (true)
            //{
            //    Console.Write("Input Time As UTC:");
            //    //string inputT = Console.ReadLine();
            DateTime dt = DateTime.Now.ToUniversalTime();
            string inputT = "2000-01-01T12:00:00.0000000Z";
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            DateTimeStyles style = DateTimeStyles.AdjustToUniversal;
            DateTime.TryParse(inputT, culture, style, out dt);
            dt.ToUniversalTime();
            dt = dt.AddSeconds(-69.184);
            VSOPTime vTime = new VSOPTime(dt);
            Console.WriteLine();
            Console.WriteLine("Start Substitution...");

            VSOPResult_ELL ell;
            VSOPResult_XYZ xyz;
            VSOPResult_LBR lbr;

            foreach(VSOPBody body in Enum.GetValues(typeof(VSOPBody)))
            {
                ell = vsop.GetPlanetPosition(body, vTime);
                xyz = (VSOPResult_XYZ)ell;
                lbr = (VSOPResult_LBR)ell;
                FormattedPrint(ell, vTime);
                FormattedPrint(xyz, vTime);
                FormattedPrint(lbr, vTime);
            }

           


            Console.WriteLine("Press Enter to Start Performance Test...");
            Console.ReadLine();
#if DEBUG
            var summary = BenchmarkRunner.Run<PerfTest>(new DebugBuildConfig());
#else
            var summary = BenchmarkRunner.Run<PerfTest>();
#endif
        }

        public static void FormattedPrint(VSOPResult Result, VSOPTime vtime)
        {
            Console.WriteLine("===============================================================");
            WriteColorLine(ConsoleColor.Cyan, "PLANETARY EPHEMERIS VSOP2013");
            Console.WriteLine("===============================================================");

            if (Result.CoordinatesType == CoordinatesType.Elliptic)
            {
                WriteColorLine("Coordinates Type: ", ConsoleColor.Green, "\tHeliocentric Elliptic");
                WriteColorLine("Reference Frame: ", ConsoleColor.Green, "\tDynamical Equinox and Ecliptic J2000");
                WriteColorLine("Body: ", ConsoleColor.Green, $"\t\t\t{Enum.GetName(Result.Body)}");
                WriteColorLine("At UTC: ", ConsoleColor.Green, $"\t\t{Result.Time.UTC.ToUniversalTime().ToString("o")}");
                WriteColorLine("At TDB: ", ConsoleColor.Green, $"\t\t{Result.Time.TDB.ToString("o")}");

                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine(String.Format("{0,-33}{1,30}", "semi-major axis (au)", (Result as VSOPResult_ELL).a));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "mean longitude (rad)", (Result as VSOPResult_ELL).l));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "k = e*cos(pi) (rad)", (Result as VSOPResult_ELL).k));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "h = e*sin(pi) (rad)", (Result as VSOPResult_ELL).h));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "q = sin(i/2)*cos(omega) (rad)", (Result as VSOPResult_ELL).q));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "p = sin(i/2)*sin(omega) (rad)", (Result as VSOPResult_ELL).p));
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("e:     eccentricity");
                Console.WriteLine("pi:    perihelion longitude");
                Console.WriteLine("i:     inclination");
                Console.WriteLine("omega: ascending node longitude");
                Console.WriteLine("===============================================================");
                Console.WriteLine();
            }
            else if (Result.CoordinatesType == CoordinatesType.Rectangular)
            {
                WriteColorLine("Coordinates Type: ", ConsoleColor.Green, "\t Heliocentric Rectangular");
                if (Result.InertialFrame == InertialFrame.Dynamical)
                {
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, "\tDynamical Ecliptic J2000");
                }
                else
                {
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, "\tEquatorial ICRS J2000");
                }

                WriteColorLine("Body: ", ConsoleColor.Green, $"\t\t\t{Enum.GetName(Result.Body)}");
                WriteColorLine("At UTC: ", ConsoleColor.Green, $"\t\t{Result.Time.UTC.ToUniversalTime().ToString("o")}");
                WriteColorLine("At TDB: ", ConsoleColor.Green, $"\t\t{Result.Time.TDB.ToString("o")}");

                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine(String.Format("{0,-33}{1,30}", "position x (au)", (Result as VSOPResult_XYZ).x));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "position y (au)", (Result as VSOPResult_XYZ).y));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "position z (au)", (Result as VSOPResult_XYZ).z));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "velocity x (au/day)", (Result as VSOPResult_XYZ).dx));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "velocity y (au/day)", (Result as VSOPResult_XYZ).dy));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "velocity z (au/day)", (Result as VSOPResult_XYZ).dz));
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine("===============================================================");
                Console.WriteLine();
            }
            else if (Result.CoordinatesType == CoordinatesType.Spherical)
            {
                WriteColorLine("Coordinates Type: ", ConsoleColor.Green, "\t Heliocentric Spherical");
                if (Result.InertialFrame == InertialFrame.Dynamical)
                {
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, "\tDynamical Ecliptic J2000");
                }
                else
                {
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, "\tEquatorial ICRS J2000");
                }

                WriteColorLine("Body: ", ConsoleColor.Green, $"\t\t\t{Enum.GetName(Result.Body)}");
                WriteColorLine("At UTC: ", ConsoleColor.Green, $"\t\t{Result.Time.UTC.ToUniversalTime().ToString("o")}");
                WriteColorLine("At TDB: ", ConsoleColor.Green, $"\t\t{Result.Time.TDB.ToString("o")}");
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine(String.Format("{0,-33}{1,30}", "longitude (rad)", (Result as VSOPResult_LBR).l));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "latitude (rad)", (Result as VSOPResult_LBR).b));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "radius (au)", (Result as VSOPResult_LBR).r));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "longitude velocity (rd/day)", (Result as VSOPResult_LBR).dl));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "latitude velocity (rd/day)", (Result as VSOPResult_LBR).db));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "radius velocity (au/day)", (Result as VSOPResult_LBR).dr));
                Console.WriteLine("===============================================================");
            }
        }

        private static void WriteColorLine(params object[] oo)
        {
            foreach (var o in oo)
                if (o == null)
                    Console.ResetColor();
                else if (o is ConsoleColor)
                    Console.ForegroundColor = (ConsoleColor)o;
                else
                    Console.Write(o.ToString());
            Console.WriteLine();
            Console.ResetColor();
        }

    }
}