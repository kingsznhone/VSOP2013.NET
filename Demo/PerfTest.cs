using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using VSOP2013;

namespace Demo
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class PerfTest
    {
        private Calculator vsop;

        private DateTime dt;
        private VSOPTime vTime;

        public PerfTest()
        { vsop = new Calculator(); }

        [GlobalSetup]
        public void init()
        {
            dt = DateTime.Now.ToUniversalTime();
            dt.ToUniversalTime();
            dt = dt.AddSeconds(-69.184);
            vTime = new VSOPTime(dt, TimeFrame.UTC);
        }

        [Benchmark(Baseline = true)]
        public VSOPResult Compute()
        {
            var ell = vsop.GetPlanetPosition(VSOPBody.JUPITER, vTime);
            return ell;
        }

        [Benchmark]
        public VSOPResult Compute_Native()
        {
            var ell = vsop.GetPlanetPosition_Native(VSOPBody.JUPITER, vTime);
            return ell;
        }
    }
}