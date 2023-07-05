using System.Diagnostics;
using System.IO.Compression;
using MessagePack;

namespace VSOP2013.DataConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

            #region Read Original Data

            Stopwatch sw = new();
            sw.Start();

            List<PlanetTable> VSOP2013DATA = DataReader.ReadData();
            VSOP2013DATA = VSOP2013DATA.OrderBy(x => x.body).ToList();

            sw.Stop();
            double ticks = sw.ElapsedTicks;
            double Freq = Stopwatch.Frequency;
            double milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Data Read & Convert OK...Elapsed milliseconds: {milliseconds} ms");

            #endregion Read Original Data

            #region Dump Data

            string OutputDirPath = Directory.GetCurrentDirectory() + @"\Data";
            if (!Directory.Exists(OutputDirPath))
            {
                Directory.CreateDirectory(OutputDirPath);
            }
            DirectoryInfo OutputDir = new(Directory.GetCurrentDirectory() + @"\Data");

            sw.Restart();

            ParallelLoopResult result = Parallel.For(0, 9, ip =>
            {
                string filename = Path.Combine(OutputDir.FullName, string.Format("VSOP2013_{0}.BIN", VSOP2013DATA[ip].body.ToString()));
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
                using FileStream fs = new(filename, FileMode.OpenOrCreate);
                using BrotliStream bs = new(fs, CompressionLevel.SmallestSize);
                MessagePackSerializer.Serialize(bs, VSOP2013DATA[ip]);
            });

            sw.Stop();
            ticks = sw.ElapsedTicks;
            milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Data Dumped OK. Elapsed: {milliseconds}ms");

            #endregion Dump Data

            #region Test

            sw.Restart();
            VSOP2013DATA.Clear();
            result = Parallel.For(0, 9, ip =>
            {
                string filename = Path.Combine(OutputDir.FullName, string.Format("VSOP2013_{0}.BIN", ((VSOPBody)ip).ToString()));
                using FileStream fs = new(filename, FileMode.OpenOrCreate);
                using BrotliStream bs = new(fs, CompressionMode.Decompress);
                VSOP2013DATA.Add(MessagePackSerializer.Deserialize<PlanetTable>(bs));
            });

            sw.Stop();
            ticks = sw.ElapsedTicks;
            milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Dump Data Reload Test OK. Elapsed: {milliseconds}ms");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            #endregion Test
        }
    }
}