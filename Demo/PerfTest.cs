using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using VSOP2013;

namespace Demo
{
    [SimpleJob(RuntimeMoniker.Net70)]
    [MemoryDiagnoser]
    public class PerfTest
    {
        private Calculator vsop;

        private DateTime dt;
        VSOPTime vTime;
        public PerfTest() { vsop = new Calculator(); }

        [GlobalSetup]
        public void init()
        {
            dt = DateTime.Now.ToUniversalTime();
            dt.ToUniversalTime();
            dt = dt.AddSeconds(-69.184);
            vTime =new VSOPTime(dt);
        }

        [Benchmark]
        public VSOPResult Go()
        {
            var ell = vsop.GetPlanetPosition(VSOPBody.JUPITER, vTime);
            return ell;
        }

    }
}
