// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;

namespace VSOP2013.Tests;

/// <summary>
/// The widest available SIMD path must reproduce the scalar path of <see cref="Calculator"/>.
/// </summary>
[TestClass]
public class SimdConsistencyTests
{
    private const double Tolerance = 1e-12;

    private static readonly int[] _years = [1000, 1600, 1900, 2000, 2024, 2100, 2500, 3000];

    [TestMethod]
    public void SimdVariablesMatchScalar()
    {
        Calculator scalar = new(SimdMode.Scalar);
        Calculator simd = new(SimdMode.Auto);

        StringBuilder failures = new();
        int cases = 0;
        foreach (VSOPBody body in Enum.GetValues<VSOPBody>().Where(b => b != VSOPBody.SUN))
        {
            foreach (VSOPVariable variable in Enum.GetValues<VSOPVariable>())
            {
                foreach (int year in _years)
                {
                    cases++;
                    VSOPTime time = new(new DateTime(year, 6, 15, 12, 0, 0, DateTimeKind.Utc), TimeFrame.UTC);
                    double expected = scalar.GetVariable(body, variable, time);
                    double actual = simd.GetVariable(body, variable, time);

                    double diff = Math.Abs(actual - expected);
                    // L is normalized to [0, 2pi), so a value near the wrap point may land on the other side.
                    if (variable == VSOPVariable.L)
                    {
                        diff = Math.Min(diff, Math.Tau - diff);
                    }

                    if (diff > Tolerance)
                    {
                        failures.AppendLine($"{body} {variable} {year}: scalar {expected:R}, simd {actual:R}, diff {diff:R}.");
                    }
                }
            }
        }

        Assert.AreEqual(0, failures.Length, $"{simd.Mode} differs from Scalar in some of {cases} cases:{Environment.NewLine}{failures}");
    }
}
