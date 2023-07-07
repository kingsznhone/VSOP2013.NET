namespace VSOP2013
{
    public enum CoordinatesReference
    {
        EclipticHeliocentric = 0,
        EquatorialHeliocentric = 2
    }

    public enum CoordinatesType
    {
        Elliptic = 0,
        Rectangular = 1,
        Spherical = 2
    }

    public enum ReferenceFrame
    {
        /// <summary>
        /// Ecliptic Heliocentric Dynamical Frame
        /// </summary>
        DynamicalJ2000 = 0,

        /// <summary>
        /// Equatorial Heliocentric ICRS Frame
        /// </summary>
        ICRSJ2000 = 2
    }

    public abstract class VSOPResult
    {
        /// <summary>
        /// Planet of this result
        /// </summary>
        public abstract VSOPBody Body { get; }

        /// <summary>
        /// Coordinates Reference
        /// </summary>
        public abstract CoordinatesReference CoordinatesReference { get; }

        /// <summary>
        /// Coordinates Type
        /// </summary>
        public abstract CoordinatesType CoordinatesType { get; }

        /// <summary>
        /// Inertial Frame
        /// </summary>
        public abstract ReferenceFrame ReferenceFrame { get; set; }

        public abstract VSOPTime Time { get; }

        //Elliptic   Elements
        //a (au), lambda (radian), k, h, q, p
        //Dynamical Frame J2000'
        public abstract double[] Variables_ELL { get; set; }
    }
}