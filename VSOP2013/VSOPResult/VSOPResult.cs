namespace VSOP2013
{
    public enum CoordinatesType
    {
        Elliptic = 0,
        Rectangular = 1,
        Spherical = 2
    }

    public enum InertialFrame
    {
        /// <summary>
        /// Ecliptic Heliocentric Dynamical Frame
        /// </summary>
        Dynamical = 0,

        /// <summary>
        /// Equatorial Heliocentric ICRS Frame
        /// </summary>
        ICRS = 1
    }

    public enum TimeFrameReference
    {
        EclipticJ2000 = 0,
    }

    public abstract class VSOPResult
    {
        public abstract VSOPBody Body { get; }
        public abstract CoordinatesType CoordinatesType { get; }
        public abstract InertialFrame InertialFrame { get; }
        public abstract TimeFrameReference TimeFrameReference { get; }

        public abstract VSOPTime Time { get; }

        //Elliptic   Elements
        //a (au), lambda (radian), k, h, q, p
        //Dynamical Frame J2000'
        public abstract double[] Variables_ELL { get; set; }
    }
}