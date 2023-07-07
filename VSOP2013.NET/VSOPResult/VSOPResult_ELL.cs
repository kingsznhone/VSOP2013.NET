namespace VSOP2013
{
    /// <summary>
    /// Elliptic   Elements
    /// a (au), lambda (radian), k, h, q, p
    /// Dynamical Frame J2000'
    /// </summary>
    public sealed class VSOPResult_ELL : VSOPResult
    {
        public override VSOPBody Body { get; }

        public override CoordinatesReference CoordinatesReference => CoordinatesReference.EclipticHeliocentric;

        public override CoordinatesType CoordinatesType => CoordinatesType.Elliptic;

        public override ReferenceFrame ReferenceFrame
        {
            get { return ReferenceFrame.DynamicalJ2000; }
            set { throw new NotSupportedException("Can't convert reference frame on elliptic coordinate."); }
        }

        public override VSOPTime Time { get; }

        public override double[] Variables_ELL { get; set; }

        public VSOPResult_ELL(VSOPBody body, VSOPTime time, double[] ell)
        {
            Body = body;
            Time = time;
            Variables_ELL = ell;
        }

        #region elements
        /// <summary>
        /// a = semi-major axis (au)
        /// </summary>
        public double a { get => Variables_ELL[0]; }

        /// <summary>
        /// l = mean longitude (rd)
        /// </summary>
        public double l { get => Variables_ELL[1]; }

        /// <summary>
        /// k = e*cos(pi) (rd)
        /// </summary>
        public double k { get => Variables_ELL[2]; }

        /// <summary>
        /// h = e*sin(pi) (rd)
        /// </summary>
        public double h { get => Variables_ELL[3]; }

        /// <summary>
        /// q = sin(i/2)*cos(omega) (rd)
        /// </summary>
        public double q { get => Variables_ELL[4]; }

        /// <summary>
        /// p = sin(i/2)*sin(omega) (rd)
        /// </summary>
        public double p { get => Variables_ELL[5]; }
        #endregion
        public VSOPResult_XYZ ToXYZ()
        {
            return new VSOPResult_XYZ(this);
        }

        public VSOPResult_LBR ToLBR()
        {
            return new VSOPResult_LBR(this);
        }
    }
}