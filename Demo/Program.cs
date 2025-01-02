using System;
using System.Globalization;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using VSOP2013;

namespace Demo
{
    internal class Program
    {
        private static readonly Calculator s_vsop = new Calculator();

        private static async Task Main(string[] args)
        {
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
            VSOPTime vTime = new VSOPTime(dt, TimeFrame.UTC);
            Console.WriteLine();
            Console.WriteLine("Start Substitution...");

            VSOPResult_ELL ell;
            VSOPResult_XYZ xyz;
            VSOPResult_LBR lbr;

            var debug = await s_vsop.GetPlanetPositionAsync(VSOPBody.EMB, vTime);

            var debug2 = await s_vsop.GetVariableAsync (VSOPBody.EMB, 0, vTime);
            ell = s_vsop.GetPlanetPosition(VSOPBody.EMB, vTime);
            FormattedPrint(ell, vTime);
            //xyz = (VSOPResult_XYZ)ell;
            //FormattedPrint(xyz, vTime);
            //xyz.ReferenceFrame = ReferenceFrame.ICRSJ2000;
            //FormattedPrint(xyz, vTime);

            //lbr = (VSOPResult_LBR)ell;
            //FormattedPrint(lbr, vTime);
            //lbr.ReferenceFrame = ReferenceFrame.ICRSJ2000;
            //FormattedPrint(lbr, vTime);
            //for (int i = 0; i < 1000; i++)
            //{
            //    foreach (VSOPBody body in Enum.GetValues(typeof(VSOPBody)))
            //    {
            //        ell = s_vsop.GetPlanetPosition(body, vTime);
            //    }
            //}

            Console.WriteLine("Press Enter to Start Performance Test...");
            Console.ReadLine();
#if DEBUG
            var summary = BenchmarkRunner.Run<PerfTest>(new DebugBuildConfig());
#else
var summary = BenchmarkRunner.Run<PerfTest>();
#endif
            Console.ReadLine();
        }

        public static void FormattedPrint(VSOPResult Result, VSOPTime vtime)
        {
            Console.WriteLine("===============================================================");
            WriteColorLine(ConsoleColor.Green, "PLANETARY EPHEMERIS VSOP2013");
            Console.WriteLine("===============================================================");
            WriteColorLine("Body: ", ConsoleColor.Green, $"\t\t\t{Enum.GetName(Result.Body)}");
            switch (Result.CoordinatesType)
            {
                case CoordinatesType.Elliptic:
                    WriteColorLine("Coordinates Type: ", ConsoleColor.Green, $"\tElliptic Elements");
                    break;

                case CoordinatesType.Rectangular:
                    WriteColorLine("Coordinates Type: ", ConsoleColor.Green, $"\tCartesian Coordinate");
                    break;

                case CoordinatesType.Spherical:
                    WriteColorLine("Coordinates Type: ", ConsoleColor.Green, $"\tSpherical Coordinate");
                    break;
            }
            switch (Result.CoordinatesReference)
            {
                case CoordinatesReference.EclipticHeliocentric:
                    WriteColorLine("Coordinates Reference: ", ConsoleColor.Green, $"\tEcliptic Heliocentric");
                    break;

                case CoordinatesReference.EquatorialHeliocentric:
                    WriteColorLine("Coordinates Reference: ", ConsoleColor.Green, $"\tEquatorial Heliocentric");
                    break;
            }
            switch (Result.ReferenceFrame)

            {
                case ReferenceFrame.DynamicalJ2000:
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, $"\tDynamical equinox and ecliptic J2000");
                    break;

                case ReferenceFrame.ICRSJ2000:
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, $"\tICRS equinox and ecliptic J2000");
                    break;
            }
            WriteColorLine("At UTC: ", ConsoleColor.Green, $"\t\t{Result.Time.UTC.ToUniversalTime():o}");
            WriteColorLine("At TDB: ", ConsoleColor.Green, $"\t\t{Result.Time.TDB:o}");

            if (Result.CoordinatesType == CoordinatesType.Elliptic)
            {
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
                Console.WriteLine("---------------------------------------------------------------");
                Console.WriteLine(String.Format("{0,-33}{1,30}", "longitude (rad)", (Result as VSOPResult_LBR).l));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "latitude (rad)", (Result as VSOPResult_LBR).b));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "radius (au)", (Result as VSOPResult_LBR).r));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "longitude velocity (rd/day)", (Result as VSOPResult_LBR).dl));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "latitude velocity (rd/day)", (Result as VSOPResult_LBR).db));
                Console.WriteLine(String.Format("{0,-33}{1,30}", "radius velocity (au/day)", (Result as VSOPResult_LBR).dr));
                Console.WriteLine("===============================================================");
                Console.WriteLine();
            }
        }

        private static void WriteColorLine(params object[] flow)
        {
            foreach (var element in flow)
                if (element == null)
                    Console.ResetColor();
                else if (element is ConsoleColor color)
                    Console.ForegroundColor = color;
                else
                    Console.Write(element.ToString());
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}
