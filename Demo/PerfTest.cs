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
        private Calculator _vsop = null!;

        private DateTime _dt;
        private VSOPTime _vTime;

        [Params(SimdMode.Scalar, SimdMode.Vector128, SimdMode.Vector256, SimdMode.Vector512)]
        public SimdMode Mode { get; set; }

        [GlobalSetup]
        public void init()
        {
            _vsop = new Calculator(Mode);
            _dt = DateTime.Now.ToUniversalTime();
            _dt.ToUniversalTime();
            _dt = _dt.AddSeconds(-69.184);
            _vTime = new VSOPTime(_dt, TimeFrame.UTC);
        }

        [Benchmark]
        public VSOPResult Compute()
        {
            var ell = _vsop.GetPlanetPosition(VSOPBody.EMB, _vTime);
            return ell;
        }
    }
}
