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
            s_upGradeFuncs = new List<Func<DateTime, DateTime>>(
                new Func<DateTime, DateTime>[] { UTCtoTAI, TAItoTT, TTtoTDB });

        private static List<Func<DateTime, DateTime>> s_downGradeFuncs = new List<Func<DateTime, DateTime>>(
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
                    dt = s_upGradeFuncs[(int)SourceFrame](dt);
                    SourceFrame += 1;
                }
                else if (TargetFrame < SourceFrame)
                {
                    dt = s_downGradeFuncs[(int)SourceFrame - 1](dt);
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
            double j2000 = 2451545.0d;
            // TDB from J2000  = OADate + JD - j2000(JD)
            return dt.ToOADate() + 2415018.5d - j2000;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="J2000"></param>
        /// <returns>Datetime in TDB Frame</returns>
        public static DateTime FromJ2000(double J2000)
        {
            double j2000 = 2451545.0d;

            return DateTime.FromOADate(J2000 + j2000 - 2415018.5).ToUniversalTime();
        }

        #endregion JulianDate Convert
    }
}
