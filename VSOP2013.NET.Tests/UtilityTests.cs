using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VSOP2013.Tests;

[TestClass]
public class UtilityTests
{
    private const double TightTolerance = 1e-13;

    [TestMethod]
    public void LBRtoXYZ_MatchesAnalyticalJacobian()
    {
        double l = 0.7;
        double b = -0.3;
        double r = 2.4;
        double dl = 0.012;
        double db = -0.004;
        double dr = 0.02;

        double[] actual = Utility.LBRtoXYZ([l, b, r, dl, db, dr]);
        double cosL = Math.Cos(l);
        double sinL = Math.Sin(l);
        double cosB = Math.Cos(b);
        double sinB = Math.Sin(b);
        double[] expected =
        [
            r * cosB * cosL,
            r * cosB * sinL,
            r * sinB,
            cosB * cosL * dr - r * sinB * cosL * db - r * cosB * sinL * dl,
            cosB * sinL * dr - r * sinB * sinL * db + r * cosB * cosL * dl,
            sinB * dr + r * cosB * db
        ];

        AssertComponents(expected, actual, TightTolerance);
    }

    [TestMethod]
    public void SphericalAndCartesianConversions_RoundTripPositionAndVelocity()
    {
        double[] expected = [0.8, -1.2, 0.4, 0.003, -0.007, 0.002];

        double[] lbr = Utility.XYZtoLBR(expected);
        double[] actual = Utility.LBRtoXYZ(lbr);

        AssertComponents(expected, actual, TightTolerance);
    }

    [TestMethod]
    public void XYZtoELL_RecoversIndependentlyConstructedEllipticElements()
    {
        const double a = 1.524;
        const double eccentricity = 0.0934;
        const double perihelionLongitude = 1.1;
        const double trueAnomaly = 2.0;
        const double q = 0.02;
        const double p = -0.01;

        double[] xyz = CreateCartesianState(
            VSOPBody.MARS,
            a,
            eccentricity,
            perihelionLongitude,
            trueAnomaly,
            q,
            p);
        double eccentricAnomaly = Math.Atan2(
            Math.Sqrt(1.0 - eccentricity * eccentricity) * Math.Sin(trueAnomaly),
            eccentricity + Math.Cos(trueAnomaly));
        double expectedL = NormalizeAngle(
            eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly) + perihelionLongitude);

        double[] actual = Utility.XYZtoELL(VSOPBody.MARS, xyz);

        AssertClose(a, actual[0], TightTolerance);
        AssertClose(expectedL, actual[1], TightTolerance);
        AssertClose(eccentricity * Math.Cos(perihelionLongitude), actual[2], TightTolerance);
        AssertClose(eccentricity * Math.Sin(perihelionLongitude), actual[3], TightTolerance);
        AssertClose(q, actual[4], TightTolerance);
        AssertClose(p, actual[5], TightTolerance);
    }

    [TestMethod]
    public void ELLtoXYZ_MatchesIndependentlyConstructedCartesianState()
    {
        const double a = 1.524;
        const double eccentricity = 0.0934;
        const double perihelionLongitude = 1.1;
        const double trueAnomaly = 2.0;
        const double q = 0.02;
        const double p = -0.01;
        double eccentricAnomaly = Math.Atan2(
            Math.Sqrt(1.0 - eccentricity * eccentricity) * Math.Sin(trueAnomaly),
            eccentricity + Math.Cos(trueAnomaly));
        double meanLongitude = NormalizeAngle(
            eccentricAnomaly - eccentricity * Math.Sin(eccentricAnomaly) + perihelionLongitude);
        double[] ell =
        [
            a,
            meanLongitude,
            eccentricity * Math.Cos(perihelionLongitude),
            eccentricity * Math.Sin(perihelionLongitude),
            q,
            p
        ];
        double[] expected = CreateCartesianState(
            VSOPBody.MARS,
            a,
            eccentricity,
            perihelionLongitude,
            trueAnomaly,
            q,
            p);

        double[] actual = Utility.ELLtoXYZ(VSOPBody.MARS, ell);

        AssertComponents(expected, actual, 1e-12);
    }

    [DataTestMethod]
    [DataRow(-25.0)]
    [DataRow(0.25)]
    [DataRow(40.0)]
    public void EllipticConversions_RoundTripWithNormalizedLongitude(double longitude)
    {
        double[] expected = [1.524, longitude, 0.08, -0.04, -0.02, 0.01];

        double[] xyz = Utility.ELLtoXYZ(VSOPBody.MARS, expected);
        double[] actual = Utility.XYZtoELL(VSOPBody.MARS, xyz);

        AssertClose(expected[0], actual[0], 1e-12);
        AssertClose(NormalizeAngle(expected[1]), actual[1], 1e-12);
        for (int index = 2; index < expected.Length; index++)
        {
            AssertClose(expected[index], actual[index], 1e-12);
        }
        Assert.IsTrue(actual[1] >= 0.0 && actual[1] < Math.Tau);
    }

    [TestMethod]
    public void ReferenceFrameConversions_RoundTripPositionAndVelocity()
    {
        double[] expected = [0.8, -1.2, 0.4, 0.003, -0.007, 0.002];

        double[] icrs = Utility.DynamicaltoICRS(expected);
        double[] actual = Utility.ICRStoDynamical(icrs);

        AssertComponents(expected, actual, 1e-14);
    }

    private static double[] CreateCartesianState(
        VSOPBody body,
        double a,
        double eccentricity,
        double perihelionLongitude,
        double trueAnomaly,
        double q,
        double p)
    {
        double chi = Math.Sqrt(1.0 - q * q - p * p);
        double[] e1 = [1.0 - 2.0 * p * p, 2.0 * p * q, -2.0 * p * chi];
        double[] e2 = [2.0 * p * q, 1.0 - 2.0 * q * q, 2.0 * q * chi];
        double cosPerihelion = Math.Cos(perihelionLongitude);
        double sinPerihelion = Math.Sin(perihelionLongitude);
        double trueLongitude = perihelionLongitude + trueAnomaly;
        double radius = a * (1.0 - eccentricity * eccentricity)
            / (1.0 + eccentricity * Math.Cos(trueAnomaly));
        double mu = Utility.GM[body] + Utility.GM[VSOPBody.SUN];
        double speedScale = Math.Sqrt(mu / (a * (1.0 - eccentricity * eccentricity)));
        double[] perihelionDirection =
        [
            cosPerihelion * e1[0] + sinPerihelion * e2[0],
            cosPerihelion * e1[1] + sinPerihelion * e2[1],
            cosPerihelion * e1[2] + sinPerihelion * e2[2]
        ];
        double[] transverseDirection =
        [
            -sinPerihelion * e1[0] + cosPerihelion * e2[0],
            -sinPerihelion * e1[1] + cosPerihelion * e2[1],
            -sinPerihelion * e1[2] + cosPerihelion * e2[2]
        ];
        double cosTrueLongitude = Math.Cos(trueLongitude);
        double sinTrueLongitude = Math.Sin(trueLongitude);
        double sinTrueAnomaly = Math.Sin(trueAnomaly);
        double transverseVelocityFactor = eccentricity + Math.Cos(trueAnomaly);

        return
        [
            radius * (cosTrueLongitude * e1[0] + sinTrueLongitude * e2[0]),
            radius * (cosTrueLongitude * e1[1] + sinTrueLongitude * e2[1]),
            radius * (cosTrueLongitude * e1[2] + sinTrueLongitude * e2[2]),
            speedScale * (-sinTrueAnomaly * perihelionDirection[0] + transverseVelocityFactor * transverseDirection[0]),
            speedScale * (-sinTrueAnomaly * perihelionDirection[1] + transverseVelocityFactor * transverseDirection[1]),
            speedScale * (-sinTrueAnomaly * perihelionDirection[2] + transverseVelocityFactor * transverseDirection[2])
        ];
    }

    private static double NormalizeAngle(double angle)
    {
        return (angle % Math.Tau + Math.Tau) % Math.Tau;
    }

    private static void AssertComponents(double[] expected, double[] actual, double tolerance)
    {
        Assert.AreEqual(expected.Length, actual.Length);
        for (int index = 0; index < expected.Length; index++)
        {
            AssertClose(expected[index], actual[index], tolerance);
        }
    }

    private static void AssertClose(double expected, double actual, double tolerance)
    {
        Assert.IsTrue(
            Math.Abs(actual - expected) <= tolerance,
            $"Expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
    }
}
