namespace VSOP2013
{
    public class VSOPResult_XYZ : VSOPResult
    {
        public override VSOPBody Body { get; }

        public override CoordinatesType CoordinatesType => CoordinatesType.Rectangular;

        private InertialFrame _inertialFrame;
        public override InertialFrame InertialFrame
        { get { return _inertialFrame; } }

        public override TimeFrameReference TimeFrameReference => TimeFrameReference.EclipticJ2000;

        public override VSOPTime Time { get; }

        public override double[] Variables_ELL { get; set; }

        public double[] Variables_XYZ { get; set; }

        public VSOPResult_XYZ(VSOPBody body, VSOPTime time, double[] ell)
        {
            Body = body;
            Time = time;
            _inertialFrame = InertialFrame.Dynamical;
            Variables_ELL = ell;
            Variables_XYZ = Utility.ELLtoXYZ(body, ell);
        }

        /// <summary>
        /// a = semi-major axis (au)
        /// </summary>
        public double x { get => Variables_XYZ[0]; }

        /// <summary>
        /// l = mean longitude (rd)
        /// </summary>
        public double y { get => Variables_XYZ[1]; }

        /// <summary>
        /// k = e*cos(pi) (rd)
        /// </summary>
        public double z { get => Variables_XYZ[2]; }

        /// <summary>
        /// h = e*sin(pi) (rd)
        /// </summary>
        public double dx { get => Variables_XYZ[3]; }

        /// <summary>
        /// q = sin(i/2)*cos(omega) (rd)
        /// </summary>
        public double dy { get => Variables_XYZ[4]; }

        /// <summary>
        /// p = sin(i/2)*sin(omega) (rd)
        /// </summary>
        public double dz { get => Variables_XYZ[5]; }

        public void ToICRSFrame()
        {
            if (InertialFrame == InertialFrame.Dynamical)
            {
                _inertialFrame = InertialFrame.ICRS;
                Variables_XYZ = Utility.DynamicaltoICRS(Variables_XYZ);
            }
        }

        public void ToDynamicalFrame()
        {
            if (InertialFrame == InertialFrame.ICRS)
            {
                _inertialFrame = InertialFrame.Dynamical;
                Variables_XYZ = Utility.ICRStoDynamical(Variables_XYZ);
            }
        }

        public static explicit operator VSOPResult_LBR(VSOPResult_XYZ XYZ)
        {
            double[] v = Utility.ELLtoLBR(XYZ.Body, XYZ.Variables_ELL);
            var result = new VSOPResult_LBR(XYZ.Body, XYZ.Time, v);
            if (XYZ.InertialFrame == InertialFrame.Dynamical)
            {
                return result;
            }
            else
            {
                result.ToICRSFrame();
                return result;
            }
        }

        public static explicit operator VSOPResult_ELL(VSOPResult_XYZ XYZ)
        {
            return new VSOPResult_ELL(XYZ.Body, XYZ.Time, XYZ.Variables_ELL);
        }
    }
}