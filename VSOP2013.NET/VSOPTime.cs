// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json.Serialization;

namespace VSOP2013
{
    public enum TimeFrame
    {
        UTC = 0,
        TAI = 1,
        TT = 2,
        TDB = 3
    }

    public class VSOPTime
    {
        [JsonIgnore]
        public DateTime _dt;

        /// <summary>
        /// UTC:Coordinated Universal Time
        /// </summary>
        public DateTime UTC => _dt;

        /// <summary>
        /// TAI:International Atomic Time
        /// </summary>
        public DateTime TAI => ChangeFrame(_dt, TimeFrame.UTC, TimeFrame.TAI);

        /// <summary>
        /// TT :Terrestrial Time (aka. TDT)
        /// </summary>
        public DateTime TT => ChangeFrame(_dt, TimeFrame.UTC, TimeFrame.TT);

        /// <summary>
        /// TDB:Barycentric Dynamical Time
        /// </summary>
        public DateTime TDB => ChangeFrame(_dt, TimeFrame.UTC, TimeFrame.TDB);

        /// <summary>
        /// Get JD from TDB
        /// </summary>
        public double J2000 => VSOPTime.ToJ2000(TDB);

        private static List<Func<DateTime, DateTime>>
            _upGradeFuncs = new List<Func<DateTime, DateTime>>(
                new Func<DateTime, DateTime>[] { UTCtoTAI, TAItoTT, TTtoTDB });

        private static List<Func<DateTime, DateTime>> _downGradeFuncs = new List<Func<DateTime, DateTime>>(
                new Func<DateTime, DateTime>[] { TAItoUTC, TTtoTAI, TDBtoTT });

        public VSOPTime(DateTime dt, TimeFrame sourceframe)
        {
            _dt = ChangeFrame(dt, sourceframe, TimeFrame.UTC);
            this._dt = _dt.ToUniversalTime();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="dt">DateTime in UTC Frame</param>
        /// <param name="TargetFrame"></param>
        /// <returns></returns>
        public static DateTime ChangeFrame(DateTime dt, TimeFrame SourceFrame, TimeFrame TargetFrame)
        {
            while (SourceFrame != TargetFrame)
            {
                if (TargetFrame > SourceFrame)
                {
                    dt = _upGradeFuncs[(int)SourceFrame](dt);
                    SourceFrame += 1;
                }
                else if (TargetFrame < SourceFrame)
                {
                    dt = _downGradeFuncs[(int)SourceFrame - 1](dt);
                    SourceFrame -= 1;
                }
            }
            return dt;
        }

        #region UTC To TDB

        private static DateTime UTCtoTAI(DateTime UTC)
        {
            return UTC.AddSeconds(37);
        }

        private static DateTime TAItoTT(DateTime TAI)
        {
            return TAI.AddSeconds(32.184);
        }

        private static DateTime TTtoTDB(DateTime TT)
        {
            //Error btw TT&TDB is so small that can be ignored.
            return TT;
        }

        #endregion UTC To TDB

        #region TDB to UTC

        private static DateTime TDBtoTT(DateTime TDB)
        {
            return TDB;
        }

        private static DateTime TTtoTAI(DateTime TT)
        {
            return TT.AddSeconds(-32.184);
        }

        private static DateTime TAItoUTC(DateTime TAI)
        {
            return TAI.AddSeconds(-37);
        }

        #endregion TDB to UTC

        #region JulianDate Convert

        /// <summary>
        ///
        /// </summary>
        /// <param name="dt">Barycentric Dynamical Time</param>
        /// <returns>JulianDate 2000 in double</returns>
        public static double ToJ2000(DateTime dt)
        {
            //Input TDB
            // OADate is discontinuous before 1899-12-30, so use ticks instead.
            return (double)(dt.Ticks - _j2000Ticks) / TimeSpan.TicksPerDay;
        }

        public static double ToJulianDate(DateTime dt)
        {
            return ToJ2000(dt) + 2451545.0d;
        }

        private static readonly long _j2000Ticks = new DateTime(2000, 1, 1, 12, 0, 0).Ticks;

        /// <summary>
        ///
        /// </summary>
        /// <param name="J2000"></param>
        /// <returns>Datetime in TDB Frame</returns>
        public static DateTime FromJ2000(double J2000)
        {
            return new DateTime(_j2000Ticks + (long)Math.Round(J2000 * TimeSpan.TicksPerDay), DateTimeKind.Utc);
        }

        #endregion JulianDate Convert
    }
}
