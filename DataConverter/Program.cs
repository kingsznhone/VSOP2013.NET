using MessagePack;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace VSOP2013.DataConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);

            #region Read Original Data

            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<PlanetTable> VSOP2013DATA = DataReader.ReadData();

            sw.Stop();
            double ticks = sw.ElapsedTicks;
            double Freq = Stopwatch.Frequency;
            double milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Data Read OK...Elapsed milliseconds: {milliseconds} ms");
            Console.WriteLine("Press Enter to dump data...");
            Console.ReadLine();

            #endregion Read Original Data

            #region Dump Data

            string OutputDirPath = Directory.GetCurrentDirectory() + @"\Data";
            if (!Directory.Exists(OutputDirPath))
            {
                Directory.CreateDirectory(OutputDirPath);
            }
            DirectoryInfo OutputDir = new DirectoryInfo(Directory.GetCurrentDirectory() + @"\Data");

            sw.Restart();

            ParallelLoopResult result = Parallel.For(0, 9, ip =>
            {
                string filename = Path.Combine(OutputDir.FullName, string.Format("VSOP2013_{0}.BIN", VSOP2013DATA[ip].body.ToString()));
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    MessagePackSerializer.Serialize(fs, VSOP2013DATA[ip], lz4Options);
                }
            });

            sw.Stop();
            ticks = sw.ElapsedTicks;
            milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Data Dumped OK. Elapsed: {milliseconds}ms");
            Console.WriteLine("Press Enter to test dumped data...");
            Console.ReadLine();

            #endregion Dump Data

            #region Test

            sw.Restart();

            result = Parallel.For(0, 9, ip =>
            {
                string filename = Path.Combine(OutputDir.FullName, string.Format("VSOP2013_{0}.BIN", ((VSOPBody)ip).ToString()));
                using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate))
                {

                    VSOP2013DATA.Add(MessagePackSerializer.Deserialize<PlanetTable>(fs, lz4Options));
                }
            });

            sw.Stop();
            ticks = sw.ElapsedTicks;
            milliseconds = (ticks / Freq) * 1000;
            Console.WriteLine($"Data Reload Test OK. Elapsed: {milliseconds}ms");
            Console.ReadLine();

            #endregion Test
        }
    }
}