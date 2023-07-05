namespace VSOP2013
{
    public class VSOPResult_LBR : VSOPResult
    {
        public override VSOPBody Body { get; }

        public override CoordinatesType CoordinatesType => CoordinatesType.Spherical;

        private InertialFrame _inertialFrame;
        public override InertialFrame InertialFrame
        { get { return _inertialFrame; } }

        public override TimeFrameReference TimeFrameReference => TimeFrameReference.EclipticJ2000;

        public override VSOPTime Time { get; }

        public override double[] Variables_ELL { get; set; }

        public double[] Variables_LBR { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="body"></param>
        /// <param name="time"></param>
        /// <param name="ell">Elliptic elements</param>
        public VSOPResult_LBR(VSOPBody body, VSOPTime time, double[] ell)
        {
            Body = body;
            Time = time;
            _inertialFrame = InertialFrame.Dynamical;
            Variables_ELL = ell;
            Variables_LBR = Utility.ELLtoLBR(body, ell);
        }

        /// <summary>
        /// a = semi-major axis (au)
        /// </summary>
        public double l { get => Variables_LBR[0]; }

        /// <summary>
        /// l = mean longitude (rd)
        /// </summary>
        public double b { get => Variables_LBR[1]; }

        /// <summary>
        /// k = e*cos(pi) (rd)
        /// </summary>
        public double r { get => Variables_LBR[2]; }

        /// <summary>
        /// h = e*sin(pi) (rd)
        /// </summary>
        public double dl { get => Variables_LBR[3]; }

        /// <summary>
        /// q = sin(i/2)*cos(omega) (rd)
        /// </summary>
        public double db { get => Variables_LBR[4]; }

        /// <summary>
        /// p = sin(i/2)*sin(omega) (rd)
        /// </summary>
        public double dr { get => Variables_LBR[5]; }

        public void ToICRSFrame()
        {
            if (InertialFrame == InertialFrame.Dynamical)
            {
                _inertialFrame = InertialFrame.ICRS;
                Variables_LBR = Utility.DynamicaltoICRS(Utility.ELLtoXYZ(this.Body, Variables_ELL));
            }
        }

        public void ToDynamicalFrame()
        {
            if (InertialFrame == InertialFrame.ICRS)
            {
                _inertialFrame = InertialFrame.Dynamical;
                Variables_LBR = Utility.ELLtoXYZ(this.Body, Variables_ELL);
            }
        }

        public static explicit operator VSOPResult_XYZ(VSOPResult_LBR LBR)
        {
            double[] v = Utility.ELLtoLBR(LBR.Body, LBR.Variables_ELL);
            var result = new VSOPResult_XYZ(LBR.Body, LBR.Time, v);
            if (LBR.InertialFrame == InertialFrame.Dynamical)
            {
                return result;
            }
            else
            {
                result.ToICRSFrame();
                return result;
            }
        }

        public static explicit operator VSOPResult_ELL(VSOPResult_LBR LBR)
        {
            return new VSOPResult_ELL(LBR.Body, LBR.Time, LBR.Variables_ELL);
        }
    }
}