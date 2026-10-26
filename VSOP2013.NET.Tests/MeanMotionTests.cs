// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.IO.Compression;
using MemoryPack;

namespace VSOP2013.Tests;

/// <summary>
/// Calculator takes the mean motion from the first term of L at power 1;
/// this guards that the data still matches the official freqpla constants of VSOP2013.f.
/// </summary>
[TestClass]
public class MeanMotionTests
{
    private static Dictionary<VSOPBody, PlanetTable> _data = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
        using FileStream fs = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "VSOP2013.BR"));
        using BrotliStream bs = new(fs, CompressionMode.Decompress);
        using MemoryStream ms = new();
        bs.CopyTo(ms);
        _data = MemoryPackSerializer.Deserialize<List<PlanetTable>>(ms.ToArray())!.ToDictionary(p => p.Body);
    }

    [TestMethod]
    [DataRow(VSOPBody.MERCURY, 0.2608790314068555e5d)]
    [DataRow(VSOPBody.VENUS, 0.1021328554743445e5d)]
    [DataRow(VSOPBody.EMB, 0.6283075850353215e4d)]
    [DataRow(VSOPBody.MARS, 0.3340612434145457e4d)]
    [DataRow(VSOPBody.JUPITER, 0.5296909615623250e3d)]
    [DataRow(VSOPBody.SATURN, 0.2132990861084880e3d)]
    [DataRow(VSOPBody.URANUS, 0.7478165903077800e2d)]
    [DataRow(VSOPBody.NEPTUNE, 0.3813297222612500e2d)]
    [DataRow(VSOPBody.PLUTO, 0.2533566020437000e2d)]
    public void FirstLPower1TermIsOfficialMeanMotion(VSOPBody body, double freqpla)
    {
        Term first = _data[body].variables[VSOPVariable.L].PowerTables[1].Terms[0];

        Assert.AreEqual((0d, 0d, 0d, freqpla), (first.aa, first.bb, first.ss, first.cc));
    }
}
