using System;
using System.Collections.Generic;

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
        private DateTime _dt { get; set; }

        /// <summary>
        /// UTC:Coordinated Universal Time
        /// </summary>
        public DateTime UTC { get { return _dt; } }

        /// <summary>
        /// TAI:International Atomic Time
        /// </summary>
        public DateTime TAI { get { return ChangeFrame(_dt, TimeFrame.TAI); } }

        /// <summary>
        /// TT :Terrestrial Time (aka. TDT)
        /// </summary>
        public DateTime TT { get { return ChangeFrame(_dt, TimeFrame.TT); } }

        /// <summary>
        /// TDB:Barycentric Dynamical Time
        /// </summary>
        public DateTime TDB { get { return ChangeFrame(_dt, TimeFrame.TDB); } }

        private List<Func<DateTime, DateTime>> UpGradeFuncs;
        private List<Func<DateTime, DateTime>> DownGradeFuncs;

        public VSOPTime(DateTime UTC)
        {
            if (UTC.Kind != DateTimeKind.Utc)
            {
                this._dt = UTC.ToUniversalTime();
            }
            this._dt = UTC.ToUniversalTime();

            UpGradeFuncs = new List<Func<DateTime, DateTime>>(
                new Func<DateTime, DateTime>[] { UTCtoTAI, TAItoTT, TTtoTDB });

            DownGradeFuncs = new List<Func<DateTime, DateTime>>(
                new Func<DateTime, DateTime>[] { TAItoUTC, TTtoTAI, TDBtoTT });

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt">DateTime in UTC Frame</param>
        /// <param name="TargetFrame"></param>
        /// <returns></returns>
        public DateTime ChangeFrame(DateTime dt, TimeFrame TargetFrame)
        {
            var buffer = TimeFrame.UTC;
            while (buffer != TargetFrame)
            {
                if (TargetFrame > buffer)
                {
                    dt = UpGradeFuncs[(int)buffer](dt);
                    buffer += 1;
                }
                else if (TargetFrame < buffer)
                {
                    dt = DownGradeFuncs[(int)buffer - 1](dt);
                    buffer -= 1;
                }
            }
            return dt;
        }


        #region UTC To TDB

        public static DateTime UTCtoTAI(DateTime UTC)
        {
            return UTC.AddSeconds(37);
        }

        public static DateTime TAItoTT(DateTime TAI)
        {
            return TAI.AddSeconds(32.184);
        }

        public static DateTime TTtoTDB(DateTime TT)
        {
            //Error btw TT&TDB is so small that can be ignored.
            return TT;
        }
        #endregion

        #region TDB to UTC
        public static DateTime TDBtoTT(DateTime TDB)
        {
            return TDB;
        }

        public static DateTime TTtoTAI(DateTime TT)
        {
            return TT.AddSeconds(-32.184);
        }

        public static DateTime TAItoUTC(DateTime TAI)
        {
            return TAI.AddSeconds(-37);
        }

        #endregion

        #region JulianDate Convert

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt">Barycentric Dynamical Time</param>
        /// <returns>JulianDate in double</returns>
        public static double ToJulianDate(DateTime dt)
        {
            return dt.ToOADate() + 2415018.5d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dt">Barycentric Dynamical Time</param>
        /// <returns>JulianDate 2000</returns>
        public static double ToJulianDate2000(DateTime dt)
        {
            //Input TDB
            double j2000 = 2451545.0d;
            //OADate + JD - j2000(JD) = TDB from J2000  
            return dt.ToOADate() + 2415018.5d - j2000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="JD"></param>
        /// <returns>Datetime in TDB Frame</returns>
        public static DateTime FromJulianDate(double JD)
        {
            return DateTime.FromOADate(JD - 2415018.5);
        }
        #endregion

    }
}
