using System.Reflection;

namespace VSOP2013.DataConverter
{
    public static class DataReader
    {
        /// <summary>
        /// Mean Longitude J2000 (radian)
        /// </summary>
        private static readonly double[] ci0 =
        {
            0.4402608631669000e1d,
            0.3176134461576000e1d,
            0.1753470369433000e1,
            0.6203500014141000e1,
            0.4091360003050000e1,
            0.1713740719173000e1,
            0.5598641292287000e1,
            0.2805136360408000e1,
            0.2326989734620000e1,
            0.5995461070350000e0,
            0.8740185101070000e0,
            0.5481225395663000e1,
            0.5311897933164000e1,
            0.0e0,
            5.19846640063e0,
            1.62790513602e0,
            2.35555563875e0
        };

        /// <summary>
        /// //Mean Motions in longitude (radian/cy)
        /// </summary>
        private static readonly double[] ci1 =
        {
            0.2608790314068555e5d,
            0.1021328554743445e5d,
            0.6283075850353215e4d,
            0.3340612434145457e4d,
            0.1731170452721855e4d,
            0.1704450855027201e4d,
            0.1428948917844273e4d,
            0.1364756513629990e4d,
            0.1361923207632842e4d,
            0.5296909615623250e3d,
            0.2132990861084880e3d,
            0.7478165903077800e2d,
            0.3813297222612500e2d,
            0.3595362285049309e0d,
            77713.7714481804e0d,
            84334.6615717837e0d,
            83286.9142477147e0d
        };

        public static List<PlanetTable> ReadData()
        {
            List<PlanetTable> VSOP2013DATA = new();
            for (int ip = 0; ip < 9; ip++)
            {
                PlanetTable planet = new()
                {
                    body = (VSOPBody)ip,
                    variables = new VariableTable[6]
                };

                for (int iv = 0; iv < 6; iv++)
                {
                    planet.variables[iv].PowerTables = new PowerTable[21];
                }
                VSOP2013DATA.Add(planet);
            }

            ParallelLoopResult result = Parallel.For(0, 9, ip =>
            {
                ReadPlanet(VSOP2013DATA[ip], ip);
            });

            for (int ip = 0; ip < 9; ip++)
            {
                for (int iv = 0; iv < 6; iv++)
                {
                    VSOP2013DATA[ip].variables[iv].PowerTables = DataPruning(VSOP2013DATA[ip].variables[iv].PowerTables);
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            return VSOP2013DATA;
        }

        private static void ReadPlanet(PlanetTable Planet, int ip)
        {
            StreamReader sr;

            string line;
            {
                //var debug = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                var assembly = Assembly.GetExecutingAssembly();
                string datafilename = $"DataConverter.Resources.VSOP2013p{ip + 1}.dat";
                Stream s = assembly.GetManifestResourceStream(datafilename);

                sr = new StreamReader(s);
                while ((line = sr.ReadLine()) != null)
                {
                    Header H = ReadHeader(line);

                    Planet.body = (VSOPBody)H.ip;
                    Planet.variables[H.iv].Body = (VSOPBody)H.ip;
                    Planet.variables[H.iv].Variable = (VSOPVariable)H.iv;

                    Term[] buffer = new Term[H.nt];
                    for (int i = 0; i < H.nt; i++)
                    {
                        line = sr.ReadLine();
                        ReadTerm(line, ref buffer[i]);
                    }
                    Planet.variables[H.iv].PowerTables[H.it].Variable = (VSOPVariable)H.iv;
                    Planet.variables[H.iv].PowerTables[H.it].Power = H.it;
                    Planet.variables[H.iv].PowerTables[H.it].Body = (VSOPBody)H.ip;
                    Planet.variables[H.iv].PowerTables[H.it].Terms = buffer;
                }
                sr.Close();
            }
        }

        private static Header ReadHeader(string line)
        {
            ReadOnlySpan<char> lineSpan = line.AsSpan();
            Header H = new();
            int lineptr = 9;
            H.ip = int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim()) - 1;
            lineptr += 3;
            H.iv = int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim()) - 1;
            lineptr += 3;
            H.it = int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim());
            lineptr += 3;
            H.nt = int.Parse(lineSpan[lineptr..(lineptr + 7)].Trim());
            return H;
        }

        private static Term ReadTerm(string line, ref Term T)
        {
            ReadOnlySpan<char> lineSpan = line.AsSpan();
            int lineptr;
            Span<int> Bufferiphi = stackalloc int[17];
            int index = 0;
            double ci;

            //
            lineptr = 5;
            //T.rank = Convert.ToInt32(line[..lineptr]);
            //
            lineptr++;
            //
            for (int counter = 0; counter < 4; counter++)
            {
                var debug = lineSpan[lineptr..(lineptr + 3)].Trim();
                Bufferiphi[index] = int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim());
                index++;
                lineptr += 3;
            }
            //
            lineptr++;
            //
            for (int counter = 0; counter < 5; counter++)
            {
                Bufferiphi[index] = int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim());
                index++;
                lineptr += 3;
            }
            //
            lineptr++;
            //
            for (int counter = 0; counter < 4; counter++)
            {
                Bufferiphi[index] = int.Parse(lineSpan[lineptr..(lineptr + 4)].Trim());
                index++;
                lineptr += 4;
            }
            //
            lineptr++;
            //
            Bufferiphi[index] = int.Parse(lineSpan[lineptr..(lineptr + 6)].Trim());
            index++;
            lineptr += 6;
            //
            lineptr++;
            //
            for (int counter = 0; counter < 3; counter++)
            {
                Bufferiphi[index] = int.Parse(lineSpan[lineptr..(lineptr + 3)].Trim());
                index++;
                lineptr += 3;
            }

            ci = double.Parse(lineSpan[lineptr..(lineptr + 20)].Trim());
            lineptr += 20;
            lineptr++;
            ci *= Math.Pow(10, double.Parse(lineSpan[lineptr..(lineptr + 3)].Trim()));
            lineptr += 3;
            T.ss = ci;

            ci = double.Parse(lineSpan[lineptr..(lineptr + 20)].Trim());
            lineptr += 20;
            lineptr++;
            ci *= Math.Pow(10, double.Parse(lineSpan[lineptr..(lineptr + 3)].Trim()));

            T.cc = ci;

            T.aa = 0;
            T.bb = 0;

            for (int j = 0; j < 17; j++)
            {
                T.aa += Bufferiphi[j] * ci0[j];
                T.bb += Bufferiphi[j] * ci1[j];
            }
            return T;
        }

        private static PowerTable[] DataPruning(PowerTable[] tables)
        {
            for (int i = 0; i < tables.Length; i++)
            {
                if (tables[i].Terms is null)
                {
                    PowerTable[] result = new PowerTable[i];

                    Array.Copy(tables, result, i);
                    return result;
                }
            }
            return tables;
        }
    }
}