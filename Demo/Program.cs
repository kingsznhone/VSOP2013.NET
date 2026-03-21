using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using VSOP2013;

namespace Demo
{
    internal class Program
    {
        private static readonly Calculator _vsop = new Calculator();

        private static async Task Main(string[] args)
        {
            //Console.WriteLine("Parse UTC string that conforms to ISO 8601:  2018-08-18T07:22:16.0000000Z");

            string inputT = "2025-01-01T12:00:00.0000000Z";
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            DateTimeStyles style = DateTimeStyles.AdjustToUniversal;
            DateTime.TryParse(inputT, culture, style, out DateTime dt);
            dt = dt.AddSeconds(-69.184);
            VSOPTime vTime = new VSOPTime(dt, TimeFrame.UTC);
            Console.WriteLine();
            Console.WriteLine("Start Substitution...");

            VSOPResult ell;
            VSOPResult xyz;
            VSOPResult lbr;

            ell = _vsop.GetPlanetPosition(VSOPBody.EMB, vTime);
            FormattedPrint(ell, vTime);

            Console.WriteLine(JsonSerializer.Serialize(ell, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

            Console.WriteLine("Press Enter to Start Performance Test...");
            Console.ReadLine();
            return;
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
            switch (Result.ReferenceFrame)
            {
                case ReferenceFrame.DynamicalJ2000:
                    WriteColorLine("Coordinates Reference: ", ConsoleColor.Green, $"\tEcliptic Heliocentric");
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, $"\tDynamical equinox and ecliptic J2000");
                    break;

                case ReferenceFrame.ICRSJ2000:
                    WriteColorLine("Coordinates Reference: ", ConsoleColor.Green, $"\tEquatorial Heliocentric");
                    WriteColorLine("Reference Frame: ", ConsoleColor.Green, $"\tICRS equinox and ecliptic J2000");
                    break;
            }
            WriteColorLine("At UTC: ", ConsoleColor.Green, $"\t\t{Result.Time.UTC.ToUniversalTime():o}");
            WriteColorLine("At TDB: ", ConsoleColor.Green, $"\t\t{Result.Time.TDB:o}");
            WriteColorLine("At JD: ", ConsoleColor.Green, $"\t\t\t{VSOPTime.ToJulianDate(Result.Time.TDB)}");
            switch (Result)
            {
                case VSOPResult_ELL e:
                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "semi-major axis (au)", e.a));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "mean longitude (rad)", e.l));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "k = e*cos(pi) (rad)", e.k));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "h = e*sin(pi) (rad)", e.h));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "q = sin(i/2)*cos(omega) (rad)", e.q));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "p = sin(i/2)*sin(omega) (rad)", e.p));
                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine("e:     eccentricity");
                    Console.WriteLine("pi:    perihelion longitude");
                    Console.WriteLine("i:     inclination");
                    Console.WriteLine("omega: ascending node longitude");
                    Console.WriteLine("===============================================================");
                    Console.WriteLine();
                    break;

                case VSOPResult_XYZ r:
                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "position x (au)", r.x));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "position y (au)", r.y));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "position z (au)", r.z));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "velocity x (au/day)", r.dx));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "velocity y (au/day)", r.dy));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "velocity z (au/day)", r.dz));
                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine("===============================================================");
                    Console.WriteLine();
                    break;

                case VSOPResult_LBR s:
                    Console.WriteLine("---------------------------------------------------------------");
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "longitude (rad)", s.l));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "latitude (rad)", s.b));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "radius (au)", s.r));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "longitude velocity (rd/day)", s.dl));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "latitude velocity (rd/day)", s.db));
                    Console.WriteLine(String.Format("{0,-33}{1,30}", "radius velocity (au/day)", s.dr));
                    Console.WriteLine("===============================================================");
                    Console.WriteLine();
                    break;
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
