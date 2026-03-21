using System.Reflection;

namespace VSOP2013.DataConverter
{
    public static class DataReader
    {
        /// <summary>
        /// Mean Longitude J2000 (radian)
        /// </summary>
        private static readonly double[] _ci0 =
        {
            0.4402608631669000e1d, //Mercury
            0.3176134461576000e1d, //Venus
            0.1753470369433000e1d,  //Earth-Moon Barycenter
            0.6203500014141000e1d,  //Mars
            0.4091360003050000e1d,  //Vesta
            0.1713740719173000e1d,  //Iris
            0.5598641292287000e1d,  //Bamberga
            0.2805136360408000e1d,  //Ceres
            0.2326989734620000e1d,  //Pallas
            0.5995461070350000e0d,  //Jupiter
            0.8740185101070000e0d,  //Saturn
            0.5481225395663000e1d,  //Uranus
            0.5311897933164000e1d,  //Neptune
            0.0000000000000000e0d,
            5.1984664006300000e0d,  //Moon (D)
            1.6279051360200000e0d,  //Moon (F)
            2.3555556387500000e0d   //Moon (l)
        };

        /// <summary>
        /// Mean Motions in longitude (radian/cy)
        /// </summary>
        private static readonly double[] _ci1 =
        {
            0.2608790314068555e5d,  //Mercury
            0.1021328554743445e5d,  //Venus
            0.6283075850353215e4d,  //Earth-Moon Barycenter
            0.3340612434145457e4d,  //Mars
            0.1731170452721855e4d,  //Vesta
            0.1704450855027201e4d,  //Iris
            0.1428948917844273e4d,  //Bamberga
            0.1364756513629990e4d,  //Ceres
            0.1361923207632842e4d,  //Pallas
            0.5296909615623250e3d,  //Jupiter
            0.2132990861084880e3d,  //Saturn
            0.7478165903077800e2d,  //Uranus
            0.3813297222612500e2d,  //Neptune
            0.3595362285049309e0d,  //Pluto (Mu from TOP2013)
            77713.771448180400e0d,  //Moon (D)
            84334.661571783700e0d,  //Moon (F)
            83286.914247714700e0d   //Moon (l)
        };

        public static List<PlanetTable> ReadData()
        {
            Dictionary<VSOPBody, PlanetTable> dict = new();
            foreach (VSOPBody body in Enum.GetValues<VSOPBody>().Where(b => b != VSOPBody.SUN))
            {
                dict[body] = new PlanetTable
                {
                    Body = body,
                    variables = Enum.GetValues<VSOPVariable>()
                        .ToDictionary(v => v, _ => new VariableTable
                        {
                            PowerTables = Enumerable.Range(0, 21)
                                .Select(_ => new PowerTable())
                                .ToArray()
                        })
                };
            }

            Parallel.ForEach(Enum.GetValues<VSOPBody>().Where(b => b != VSOPBody.SUN), body =>
            {
                ReadPlanet(dict[body], body);
            });

            List<PlanetTable> VSOP2013DATA = dict.Values.OrderBy(p => p.Body).ToList();

            //Table Pruning
            foreach (var planet in VSOP2013DATA)
            {
                foreach (var variable in planet.variables.Values)
                {
                    variable.PowerTables = variable.PowerTables.TakeWhile(t => t?.Terms is not null).ToArray();
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return VSOP2013DATA;
        }

        private static void ReadPlanet(PlanetTable Planet, VSOPBody body)
        {
            //var debug = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            var assembly = Assembly.GetExecutingAssembly();
            string datafilename = $"DataConverter.Resources.VSOP2013p{(int)body}.dat";
            Stream? s = assembly.GetManifestResourceStream(datafilename);

            if (s is null)
            {
                throw new FileNotFoundException($"Resource '{datafilename}' not found.");
            }

            StreamReader sr = new StreamReader(s);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                Header H = ReadHeader(line);

                Planet.Body = H.Body;
                Planet.variables[H.Variable].Body = H.Body;
                Planet.variables[H.Variable].Variable = H.Variable;

                Term[] buffer = new Term[H.TermsCount];
                for (int i = 0; i < H.TermsCount; i++)
                {
                    line = sr.ReadLine();
                    ReadTerm(line, ref buffer[i]);
                }
                Planet.variables[H.Variable].PowerTables[H.Power].Variable = H.Variable;
                Planet.variables[H.Variable].PowerTables[H.Power].Power = H.Power;
                Planet.variables[H.Variable].PowerTables[H.Power].Body = H.Body;
                Planet.variables[H.Variable].PowerTables[H.Power].Terms = buffer;
            }
            sr.Close();
        }

        private static Header ReadHeader(string line)
        {
            ReadOnlySpan<char> lineSpan = line.AsSpan();
            Header H = new();
            int lineptr = 9;
            H.Body = (VSOPBody)int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim());
            lineptr += 3;
            H.Variable = (VSOPVariable)int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim());
            lineptr += 3;
            H.Power = int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim());
            lineptr += 3;
            H.TermsCount = int.Parse(lineSpan[lineptr..(lineptr + 7)].Trim());
            return H;
        }

        private static Term ReadTerm(ReadOnlySpan<char> line, ref Term T)
        {
            int ptr;
            Span<int> iPhi = stackalloc int[17];
            int index = 0;
            double ci;

            //
            ptr = 5;
            //T.rank = Convert.ToInt32(line[..lineptr]);
            //
            ptr++;
            //
            for (int counter = 0; counter < 4; counter++)
            {
                iPhi[index] = int.Parse(line[ptr..(ptr + 3)].Trim());
                index++;
                ptr += 3;
            }
            //
            ptr++;
            //
            for (int counter = 0; counter < 5; counter++)
            {
                iPhi[index] = int.Parse(line[ptr..(ptr + 3)].Trim());
                index++;
                ptr += 3;
            }
            //
            ptr++;
            //
            for (int counter = 0; counter < 4; counter++)
            {
                iPhi[index] = int.Parse(line[ptr..(ptr + 4)].Trim());
                index++;
                ptr += 4;
            }
            //
            ptr++;
            //
            iPhi[index] = int.Parse(line[ptr..(ptr + 6)].Trim());
            index++;
            ptr += 6;
            //
            ptr++;
            //
            for (int counter = 0; counter < 3; counter++)
            {
                iPhi[index] = int.Parse(line[ptr..(ptr + 3)].Trim());
                index++;
                ptr += 3;
            }

            ci = double.Parse(line[ptr..(ptr + 20)].Trim());
            ptr += 20;
            ptr++;
            ci *= Math.Pow(10, double.Parse(line[ptr..(ptr + 3)].Trim()));
            ptr += 3;
            T.ss = ci;

            ci = double.Parse(line[ptr..(ptr + 20)].Trim());
            ptr += 20;
            ptr++;
            ci *= Math.Pow(10, double.Parse(line[ptr..(ptr + 3)].Trim()));

            T.cc = ci;

            for (int j = 0; j < 17; j++)
            {
                T.aa += iPhi[j] * _ci0[j];
                T.bb += iPhi[j] * _ci1[j];
            }
            return T;
        }
    }
}
