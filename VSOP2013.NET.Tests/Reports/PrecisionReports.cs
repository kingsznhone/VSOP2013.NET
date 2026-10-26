// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;
using System.IO.Compression;
using System.Text;
using MemoryPack;

namespace VSOP2013.Tests.Reports;

/// <summary>
/// Diagnostic precision reports (no assertions). Each report samples +-1000 years around J2000
/// and compares a calculator configuration against the full-series double scalar reference.
/// Output files are written next to the test assembly.
/// </summary>
[TestClass]
[TestCategory("Report")]
public class PrecisionReports
{
    private const int Samples = 2001;
    private const double SpanDays = 365250d;
    private const double NearDays = 36525d;
    private const double RadToArcsec = 180d * 3600d / Math.PI;

    private static readonly double[] _truncationLevels = [0d, 1e-14, 1e-12, 1e-10, 1e-8];

    private static Calculator _reference = null!;
    private static VSOPTime[] _times = null!;
    private static (VSOPBody Body, VSOPVariable Variable)[] _groups = null!;

    public TestContext TestContext { get; set; } = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        _reference = new Calculator(SimdMode.Scalar);

        DateTime j2000 = new(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        _times = new VSOPTime[Samples];
        for (int i = 0; i < Samples; i++)
        {
            _times[i] = new VSOPTime(j2000.AddDays(-SpanDays + 2 * SpanDays * i / (Samples - 1)), TimeFrame.TDB);
        }

        _groups = Enum.GetValues<VSOPBody>().Where(b => b != VSOPBody.SUN)
            .SelectMany(b => Enum.GetValues<VSOPVariable>().Select(v => (b, v)))
            .ToArray();
    }

    [TestMethod]
    public void DoubleSimdVsDoubleScalar()
    {
        Calculator simd = new(SimdMode.Auto);
        WriteReport($"Calculator ({simd.Mode}) vs Calculator (Scalar), full series.",
            simd.GetVariable, "Report_DoubleSimd.txt");
    }

    [TestMethod]
    public void DoubleTruncatedVsDoubleFull()
    {
        Calculator truncated = new(SimdMode.Auto, truncation: CalculatorF.DefaultTruncation);
        WriteReport($"Calculator ({truncated.Mode}, rho={truncated.Truncation:E0}) vs Calculator (Scalar), full series.",
            truncated.GetVariable, "Report_DoubleTruncated.txt");
    }

    [TestMethod]
    public void FloatScalarTruncatedVsDouble()
    {
        CalculatorF calculator = new(SimdMode.Scalar);
        WriteFloatReport(calculator, "Report_FloatScalarTruncated.txt");
    }

    [TestMethod]
    public void FloatSimdTruncatedVsDouble()
    {
        CalculatorF calculator = new(SimdMode.Auto);
        WriteFloatReport(calculator, "Report_FloatSimdTruncated.txt");
    }

    [TestMethod]
    public void FloatSimdFullVsDouble()
    {
        CalculatorF calculator = new(SimdMode.Auto, truncation: 0d);
        WriteFloatReport(calculator, "Report_FloatSimdFull.txt");
    }

    [TestMethod]
    public void TermCountsByTruncation()
    {
        using FileStream fs = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "VSOP2013.BR"));
        using BrotliStream bs = new(fs, CompressionMode.Decompress);
        using MemoryStream ms = new();
        bs.CopyTo(ms);
        List<PlanetTable> planets = MemoryPackSerializer.Deserialize<List<PlanetTable>>(ms.ToArray())!;

        StringBuilder report = new();
        report.AppendLine("Terms kept with sqrt(C^2 + S^2) >= rho.");
        report.Append("Body     Var |");
        foreach (double rho in _truncationLevels)
        {
            report.Append(rho == 0d ? $" {"Full",10}" : string.Create(CultureInfo.InvariantCulture, $" {rho,10:0E0}"));
        }
        report.AppendLine();

        long[] totals = new long[_truncationLevels.Length];
        foreach (PlanetTable planet in planets.OrderBy(p => p.Body))
        {
            long[] bodyTotals = new long[_truncationLevels.Length];
            foreach (VariableTable variable in planet.variables.Values.OrderBy(v => v.Variable))
            {
                Term[] terms = variable.PowerTables.Where(p => p.Terms != null).SelectMany(p => p.Terms).ToArray();
                report.Append($"{planet.Body,-8} {variable.Variable,-3} |");
                for (int i = 0; i < _truncationLevels.Length; i++)
                {
                    double rho2 = _truncationLevels[i] * _truncationLevels[i];
                    int count = terms.Count(t => t.cc * t.cc + t.ss * t.ss >= rho2);
                    bodyTotals[i] += count;
                    totals[i] += count;
                    report.Append($" {count,10}");
                }
                report.AppendLine();
            }
            AppendCounts(report, $"{planet.Body,-8} SUM |", bodyTotals);
        }
        AppendCounts(report, "TOTAL        |", totals);

        Write(report, "Report_TermCounts.txt");
    }

    private void WriteFloatReport(CalculatorF calculator, string fileName)
    {
        WriteReport($"CalculatorF ({calculator.Mode}, rho={calculator.Truncation:E0}) vs Calculator (Scalar), full series.",
            (body, variable, time) => calculator.GetVariable(body, variable, time), fileName);
    }

    private void WriteReport(string title, Func<VSOPBody, VSOPVariable, VSOPTime, double> evaluate, string fileName)
    {
        string[] lines = new string[_groups.Length];

        // Each (body, variable) group is independent and calculators are read-only after construction.
        Parallel.For(0, _groups.Length, g =>
        {
            (VSOPBody body, VSOPVariable variable) = _groups[g];
            double[] diffs = new double[Samples];
            double nearMax = 0d;
            for (int i = 0; i < Samples; i++)
            {
                double expected = _reference.GetVariable(body, variable, _times[i]);
                double actual = evaluate(body, variable, _times[i]);
                double diff = Math.Abs(actual - expected);
                // L is normalized to [0, 2pi), so a value near the wrap point may land on the other side.
                if (variable == VSOPVariable.L)
                {
                    diff = Math.Min(diff, Math.Tau - diff);
                }
                diffs[i] = diff;
                if (Math.Abs(_times[i].J2000) <= NearDays)
                {
                    nearMax = Math.Max(nearMax, diff);
                }
            }
            double mean = diffs.Average();
            double rms = Math.Sqrt(diffs.Average(d => d * d));
            Array.Sort(diffs);
            double max = diffs[^1];
            string arcsec = variable == VSOPVariable.L
                ? string.Create(CultureInfo.InvariantCulture, $"{max * RadToArcsec,-10:E2} {nearMax * RadToArcsec,-10:E2}")
                : string.Empty;
            lines[g] = string.Create(CultureInfo.InvariantCulture,
                $"{body,-8} {variable,-3} | {max,-10:E2} {mean,-10:E2} {rms,-10:E2} {diffs[Samples / 2],-10:E2} {diffs[(int)(Samples * 0.99)],-10:E2} | {nearMax,-10:E2} | {arcsec}");
        });

        StringBuilder report = new();
        report.AppendLine(string.Create(CultureInfo.InvariantCulture, $"{title} +-1000 y around J2000 ({Samples} samples)."));
        report.AppendLine("A in au, L in rad, K/H/Q/P unitless. Near = |t-J2000| <= 100 y.");
        report.AppendLine("Body     Var | Max abs    Mean abs   RMS        P50        P99        | Near max   | L max\"     L near max\"");
        foreach (string line in lines)
        {
            report.AppendLine(line);
        }
        Write(report, fileName);
    }

    private static void AppendCounts(StringBuilder report, string label, long[] counts)
    {
        report.Append(label);
        foreach (long c in counts)
        {
            report.Append($" {c,10}");
        }
        report.AppendLine();
    }

    private void Write(StringBuilder report, string fileName)
    {
        string path = Path.Combine(AppContext.BaseDirectory, fileName);
        File.WriteAllText(path, report.ToString());
        TestContext.WriteLine(report.ToString());
        TestContext.WriteLine($"Report written to {path}");
    }
}
