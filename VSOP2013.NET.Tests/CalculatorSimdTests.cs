// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace VSOP2013.Tests;

[TestClass]
public class CalculatorSimdTests
{
    private const double Tolerance = 1e-12;

    private static Calculator _scalar = null!;
    private static Calculator _simd = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _scalar = new Calculator(SimdMode.Scalar);
        _simd = new Calculator(SimdMode.Auto);
    }

    public static IEnumerable<object[]> Cases()
    {
        int[] years = [1000, 1600, 1900, 2000, 2024, 2100, 2500, 3000];
        VSOPBody[] bodies =
        [
            VSOPBody.MERCURY, VSOPBody.VENUS, VSOPBody.EMB, VSOPBody.MARS, VSOPBody.JUPITER,
            VSOPBody.SATURN, VSOPBody.URANUS, VSOPBody.NEPTUNE, VSOPBody.PLUTO
        ];
        foreach (VSOPBody body in bodies)
        {
            foreach (VSOPVariable variable in Enum.GetValues<VSOPVariable>())
            {
                foreach (int year in years)
                {
                    yield return [body, variable, year];
                }
            }
        }
    }

    [TestMethod]
    [DynamicData(nameof(Cases))]
    public void GetVariableSimdMatchesScalar(VSOPBody body, VSOPVariable variable, int year)
    {
        VSOPTime time = new(new DateTime(year, 6, 15, 12, 0, 0, DateTimeKind.Utc), TimeFrame.UTC);

        double expected = _scalar.GetVariable(body, variable, time);
        double actual = _simd.GetVariable(body, variable, time);

        double diff = Math.Abs(actual - expected);
        // L is normalized to [0, 2pi), so a value near the wrap point may land on the other side.
        if (variable == VSOPVariable.L)
        {
            diff = Math.Min(diff, Math.Tau - diff);
        }

        Assert.IsTrue(
            diff <= Tolerance,
            $"{body} {variable} {year}: scalar {expected:R}, simd {actual:R}, diff {diff:R}.");
    }

    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    /// Diagnostic report only (no assertions). Skipped unless VSOP_RUN_REPORTS=1 is set.
    /// </summary>
    [TestMethod]
    public void ReportSimdScalarErrorDistribution()
    {
        // Uniform sampling over +-1000 years around J2000 (TDB).
        const int samples = 2001;
        const double spanDays = 365250d;
        DateTime j2000 = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        VSOPTime[] times = new VSOPTime[samples];
        for (int i = 0; i < samples; i++)
        {
            times[i] = new VSOPTime(j2000.AddDays(-spanDays + 2 * spanDays * i / (samples - 1)), TimeFrame.TDB);
        }

        System.Text.StringBuilder report = new();
        report.AppendLine("Body     Var | Max abs    Mean abs   RMS        P50        P99        | Max ULP  Mean ULP | Exact%");
        (VSOPBody Body, VSOPVariable Variable)[] groups =
            Enum.GetValues<VSOPBody>().Where(b => b != VSOPBody.SUN)
                .SelectMany(b => Enum.GetValues<VSOPVariable>().Select(v => (b, v)))
                .ToArray();
        string[] lines = new string[groups.Length];

        // Each (body, variable) group is independent and Calculator is read-only after construction.
        Parallel.For(0, groups.Length, g =>
        {
            (VSOPBody body, VSOPVariable variable) = groups[g];
            {
                double[] diffs = new double[samples];
                double[] ulps = new double[samples];
                int exact = 0;
                for (int i = 0; i < samples; i++)
                {
                    double expected = _scalar.GetVariable(body, variable, times[i]);
                    double actual = _simd.GetVariable(body, variable, times[i]);
                    double diff = Math.Abs(actual - expected);
                    if (variable == VSOPVariable.L)
                    {
                        diff = Math.Min(diff, Math.Tau - diff);
                    }
                    diffs[i] = diff;
                    ulps[i] = diff / Math.Max(Math.BitIncrement(Math.Abs(expected)) - Math.Abs(expected), double.Epsilon);
                    if (diff == 0d)
                    {
                        exact++;
                    }
                }
                Array.Sort(diffs);
                double mean = diffs.Average();
                double rms = Math.Sqrt(diffs.Average(d => d * d));
                lines[g] = string.Create(System.Globalization.CultureInfo.InvariantCulture,
                    $"{body,-8} {variable,-3} | {diffs[^1],-10:E2} {mean,-10:E2} {rms,-10:E2} {diffs[samples / 2],-10:E2} {diffs[(int)(samples * 0.99)],-10:E2} | {ulps.Max(),-8:F0} {ulps.Average(),-8:F1} | {100.0 * exact / samples:F1}");
            }
        });
        foreach (string line in lines)
        {
            report.AppendLine(line);
        }
        TestContext.WriteLine(report.ToString());
        string reportPath = Path.Combine(AppContext.BaseDirectory, "SimdScalarErrorDistribution.txt");
        File.WriteAllText(reportPath, report.ToString());
        TestContext.WriteLine($"Report written to {reportPath}");
    }
}
