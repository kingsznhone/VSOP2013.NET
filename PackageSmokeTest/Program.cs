using VSOP2013;

// Reference values: official VSOP2013.ctl, Mercury at JD 2411545.0 (TDB), dynamical frame J2000.
double[] expectedEll = [0.3870979635, 6.2605163414, 0.0452614144, 0.2005681842, 0.0405434321, 0.0457750937];
double[] expectedXyz = [0.3493879042, -0.1615770401, -0.0453430160];
const double Tolerance = 1e-9;

VSOPTime time = new(new DateTime(2000, 1, 1, 12, 0, 0, DateTimeKind.Utc).AddDays(2411545.0 - 2451545.0), TimeFrame.TDB);

Calculator calculator = new();
Console.WriteLine($"Calculator mode: {calculator.Mode}");
double[] ell = calculator.GetPlanetPosition(VSOPBody.MERCURY, time).Variables.ToArray();
double[] xyz = Utility.ELLtoXYZ(VSOPBody.MERCURY, ell);

int failures = 0;
failures += Check("ELL", ell, expectedEll, Tolerance);
failures += Check("XYZ", xyz, expectedXyz, Tolerance);

CalculatorF fast = new();
double[] fastEll = fast.GetPlanetPosition(VSOPBody.MERCURY, time).Variables.ToArray();
failures += Check("ELL (CalculatorF)", fastEll, expectedEll, 1e-2);

Console.WriteLine(failures == 0 ? "Smoke test passed." : "Smoke test FAILED.");
return failures == 0 ? 0 : 1;

static int Check(string name, double[] actual, double[] expected, double tolerance)
{
    double max = 0d;
    for (int i = 0; i < expected.Length; i++)
    {
        double diff = Math.Abs(actual[i] - expected[i]);
        // Mean longitude is an angle.
        if (name.StartsWith("ELL", StringComparison.Ordinal) && i == 1)
        {
            diff %= Math.Tau;
            diff = Math.Min(diff, Math.Tau - diff);
        }
        max = Math.Max(max, diff);
    }
    bool ok = max <= tolerance;
    Console.WriteLine($"{name}: max error {max:E3} (tolerance {tolerance:E0}) {(ok ? "OK" : "FAIL")}");
    return ok ? 0 : 1;
}
