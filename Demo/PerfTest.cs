// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using VSOP2013;

namespace Demo
{
    /// <summary>
    /// GetPlanetPosition(EMB) for {Scalar, Vector256} x {Calculator (double), CalculatorF (float)}.
    /// Ratios are computed against the double calculator within the same SIMD mode.
    /// </summary>
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByParams)]
    public class PerfTest
    {
        private Calculator _double = null!;
        private CalculatorF _float = null!;

        private VSOPTime _vTime;

        [Params(SimdMode.Scalar, SimdMode.Vector256)]
        public SimdMode Mode { get; set; }

        [GlobalSetup]
        public void init()
        {
            // Double: full series. Float: default truncation (1e-8), the intended configuration.
            _double = new Calculator(Mode);
            _float = new CalculatorF(Mode);
            _vTime = new VSOPTime(DateTime.UtcNow.AddSeconds(-69.184), TimeFrame.UTC);
        }

        [Benchmark(Baseline = true)]
        public VSOPResult Double()
        {
            return _double.GetPlanetPosition(VSOPBody.EMB, _vTime);
        }

        [Benchmark]
        public VSOPResult Float()
        {
            return _float.GetPlanetPosition(VSOPBody.EMB, _vTime);
        }
    }
}
