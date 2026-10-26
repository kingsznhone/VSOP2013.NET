// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using VSOP2013;

namespace Demo
{
    [SimpleJob(RuntimeMoniker.Net10_0)]
    [MemoryDiagnoser]
    public class PerfTest
    {
        private Calculator _full = null!;
        private Calculator _truncated = null!;

        private VSOPTime _vTime;

        [Params(SimdMode.Scalar, SimdMode.Vector256)]
        public SimdMode Mode { get; set; }

        [GlobalSetup]
        public void init()
        {
            _full = new Calculator(Mode);
            _truncated = new Calculator(Mode, truncation: 1e-14);
            _vTime = new VSOPTime(DateTime.UtcNow.AddSeconds(-69.184), TimeFrame.UTC);
        }

        [Benchmark(Baseline = true)]
        public VSOPResult Full()
        {
            return _full.GetPlanetPosition(VSOPBody.EMB, _vTime);
        }

        [Benchmark]
        public VSOPResult Truncated()
        {
            return _truncated.GetPlanetPosition(VSOPBody.EMB, _vTime);
        }
    }
}
