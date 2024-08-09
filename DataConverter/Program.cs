using System.Diagnostics;
using FastLZMA2Net;
using MemoryPack;

namespace VSOP2013.DataConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
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
            Compressor compressor = new Compressor(0, 10) { HighCompressLevel = 10 };
            string filename = Path.Combine(OutputDir.FullName, "VSOP2013.BIN");
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
            using (FileStream fs = new FileStream(filename, FileMode.CreateNew))
            {
                var compressed = compressor.Compress(MemoryPackSerializer.Serialize(VSOP2013DATA));
                fs.Write(compressed.AsSpan());
            }
            Console.WriteLine($"VSOP2013DATA: \n{filename}");
            Console.WriteLine();

            sw.Stop();
            ticks = sw.ElapsedTicks;
            milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Data Dumped OK. Elapsed: {milliseconds}ms");

            #endregion Dump Data

            #region Test

            sw.Restart();
            VSOP2013DATA.Clear();
            Decompressor decompressor = new Decompressor(0);
            var decompressed = decompressor.Decompress(File.ReadAllBytes(filename));//critical bug
            VSOP2013DATA = MemoryPackSerializer.Deserialize<List<PlanetTable>>(decompressed);

            sw.Stop();
            ticks = sw.ElapsedTicks;
            milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Dump Data Reload Test OK. Elapsed: {milliseconds}ms");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();

            // Open explorer to output path.
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = OutputDir.FullName,
                UseShellExecute = true
            });

            #endregion Test
        }
    }
}