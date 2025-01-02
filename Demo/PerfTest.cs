using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using VSOP2013;

namespace Demo
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    public class PerfTest
    {
        private Calculator _vsop;

        private DateTime _dt;
        private VSOPTime _vTime;

        public PerfTest()
        { _vsop = new Calculator(); }

        [GlobalSetup]
        public void init()
        {
            _dt = DateTime.Now.ToUniversalTime();
            _dt.ToUniversalTime();
            _dt = _dt.AddSeconds(-69.184);
            _vTime = new VSOPTime(_dt, TimeFrame.UTC);
        }

        [Benchmark(Baseline = true)]
        public VSOPResult Compute()
        {
            var ell = _vsop.GetPlanetPosition(VSOPBody.JUPITER, _vTime);
            return ell;
        }
    }
}
