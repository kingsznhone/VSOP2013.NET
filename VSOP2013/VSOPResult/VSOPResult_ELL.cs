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

        public override CoordinatesType CoordinatesType => CoordinatesType.Elliptic;

        public override InertialFrame InertialFrame => InertialFrame.Dynamical;

        public override TimeFrameReference TimeFrameReference => TimeFrameReference.EclipticJ2000;

        public override VSOPTime Time { get; }

        public override double[] Variables_ELL { get; set; }

        public VSOPResult_ELL(VSOPBody body, VSOPTime time, double[] ell)
        {
            Body = body;
            Time = time;
            Variables_ELL = ell;
        }

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

        public static explicit operator VSOPResult_XYZ(VSOPResult_ELL ELL)
        {
            return new VSOPResult_XYZ(ELL.Body, ELL.Time, ELL.Variables_ELL);
        }

        public static explicit operator VSOPResult_LBR(VSOPResult_ELL ELL)
        {
            return new VSOPResult_LBR(ELL.Body, ELL.Time, ELL.Variables_ELL);
        }
    }
}