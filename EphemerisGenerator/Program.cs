// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
using System.Text;
using VSOP2013;
namespace EphemerisGenerator
{
    internal class Program
    {
        static Calculator s_calculator = new Calculator();

        static void Main(string[] args)
        {
            StringBuilder sb = new StringBuilder();
            var enddate = new DateTime(2100, 1, 1);
            int lineCount = 0;
            using (StreamWriter writer = new StreamWriter("./ephemeris.csv",false))
            {
                writer.WriteLine("body,j2000,a,l,k,h,q,p");
                foreach (VSOPBody body in Enum.GetValues(typeof(VSOPBody)))
                {
                    VSOPTime time = new VSOPTime(new DateTime(1900, 1, 1), TimeFrame.TDB);
                    while (time.TDB < enddate)
                    {
                        VSOPResult_ELL result = s_calculator.GetPlanetPosition(body, time);
                        sb.Clear();
                        sb.Append((int)body).Append(',').Append(time.J2000).Append(',');
                        sb.Append(string.Join(", ", result.Variables_ELL));
                        writer.WriteLine(sb.ToString());
                        time._dt= time._dt.AddDays(1);
                        lineCount++;
                        if (lineCount % 1000==0)
                        {
                            Console.WriteLine($"Output {lineCount} lines...");
                        }
                    }

                }
            }
        }
    }
}
