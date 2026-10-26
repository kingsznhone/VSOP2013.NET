using System.Globalization;
using System.Text;

namespace VSOP2013.Tests;

/// <summary>
/// Validates scalar and SIMD paths against the official VSOP2013.ctl reference values.
/// </summary>
[TestClass]
public class CtlReferenceTests
{
    // .ctl values are printed with 10 decimals; allow one unit in the last place to cover ELL-to-XYZ rounding.
    private const double Tolerance = 1e-10;

    private static readonly DateTime _j2000 = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private static Calculator _scalar = null!;
    private static Calculator _simd = null!;

    public TestContext TestContext { get; set; } = null!;

    private sealed record CtlEntry(VSOPBody Body, double JulianDate, double[] Ell, double[] Xyz, double[] Icrs);

    private sealed record PathError(double Ell, double Xyz, double Icrs)
    {
        public double Max => Math.Max(Ell, Math.Max(Xyz, Icrs));
    }

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _scalar = new Calculator(SimdMode.Scalar);
        _simd = new Calculator(SimdMode.Auto);
    }

    [TestMethod]
    public void ScalarAndSimdMatchOfficialCtlValues()
    {
        List<CtlEntry> entries = ParseCtl(Path.Combine(AppContext.BaseDirectory, "Resources", "VSOP2013.ctl"));
        Assert.AreEqual(99, entries.Count);

        StringBuilder report = new();
        report.AppendLine("Body     JD(TDB)       | Scalar ELL  XYZ        ICRS       | SIMD   ELL  XYZ        ICRS");
        double scalarMax = 0d;
        double simdMax = 0d;
        foreach (CtlEntry entry in entries)
        {
            VSOPTime time = new(_j2000.AddDays(entry.JulianDate - 2451545.0d), TimeFrame.TDB);
            PathError scalar = Measure(entry, _scalar.GetPlanetPosition(entry.Body, time).Variables.ToArray());
            PathError simd = Measure(entry, _simd.GetPlanetPosition(entry.Body, time).Variables.ToArray());
            scalarMax = Math.Max(scalarMax, scalar.Max);
            simdMax = Math.Max(simdMax, simd.Max);
            report.AppendLine(string.Create(CultureInfo.InvariantCulture,
                $"{entry.Body,-8} {entry.JulianDate,-13:F1} | {scalar.Ell,-10:E2} {scalar.Xyz,-10:E2} {scalar.Icrs,-10:E2} | {simd.Ell,-10:E2} {simd.Xyz,-10:E2} {simd.Icrs,-10:E2}"));
        }
        report.AppendLine(string.Create(CultureInfo.InvariantCulture, $"Max error: scalar {scalarMax:E3}, simd {simdMax:E3}, tolerance {Tolerance:E3}"));
        TestContext.WriteLine(report.ToString());

        Assert.IsTrue(scalarMax <= Tolerance && simdMax <= Tolerance, report.ToString());
    }

    private static PathError Measure(CtlEntry entry, double[] ell)
    {
        double[] xyz = Utility.ELLtoXYZ(entry.Body, ell);
        double[] icrs = Utility.DynamicaltoICRS(xyz);
        return new PathError(MaxError(entry.Ell, ell, angleIndex: 1), MaxError(entry.Xyz, xyz), MaxError(entry.Icrs, icrs));
    }

    private static double MaxError(double[] expected, double[] actual, int angleIndex = -1)
    {
        double max = 0d;
        for (int i = 0; i < expected.Length; i++)
        {
            double diff = Math.Abs(actual[i] - expected[i]);
            if (i == angleIndex)
            {
                diff %= Math.Tau;
                diff = Math.Min(diff, Math.Tau - diff);
            }
            max = Math.Max(max, diff);
        }
        return max;
    }

    private static List<CtlEntry> ParseCtl(string path)
    {
        string[] lines = File.ReadAllLines(path);
        List<CtlEntry> entries = [];
        VSOPBody body = default;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("PLANETARY EPHEMERIS", StringComparison.Ordinal))
            {
                body = Enum.Parse<VSOPBody>(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[^1]);
            }
            else if (line.StartsWith("Julian Date JD", StringComparison.Ordinal))
            {
                double jd = double.Parse(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)[3], CultureInfo.InvariantCulture);
                entries.Add(new CtlEntry(body, jd, ParseRow(lines[i + 1]), ParseRow(lines[i + 2]), ParseRow(lines[i + 3])));
                i += 3;
            }
        }
        return entries;
    }

    private static double[] ParseRow(string line) =>
        line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => double.Parse(s, CultureInfo.InvariantCulture))
            .ToArray();
}
