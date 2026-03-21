namespace VSOP2013
{
    public sealed class VSOPResult_LBR : VSOPResult
    {
        public override VSOPBody Body { get; }

        public override CoordinatesType CoordinatesType => CoordinatesType.Spherical;

        private ReferenceFrame _referenceFrame;

        public override ReferenceFrame ReferenceFrame
        {
            get { return _referenceFrame; }
            set
            {
                if (_referenceFrame == ReferenceFrame.DynamicalJ2000 && value == ReferenceFrame.ICRSJ2000)
                {
                    Variables_LBR = Utility.XYZtoLBR(Utility.DynamicaltoICRS(Utility.LBRtoXYZ(Variables_LBR)));
                    _referenceFrame = value;
                }
                else if (_referenceFrame == ReferenceFrame.ICRSJ2000 && value == ReferenceFrame.DynamicalJ2000)
                {
                    Variables_LBR = Utility.XYZtoLBR(Utility.ICRStoDynamical(Utility.LBRtoXYZ(Variables_LBR)));
                    _referenceFrame = value;
                }
            }
        }

        public override VSOPTime Time { get; }

        public override double[] Variables_ELL { get; set; }

        public double[] Variables_LBR { get; set; }

        public VSOPResult_LBR(VSOPResult_ELL result)
        {
            Variables_ELL = result.Variables_ELL;
            _referenceFrame = result.ReferenceFrame;
            Body = result.Body;
            Time = result.Time;
            Variables_LBR = Utility.ELLtoLBR(result.Body, result.Variables_ELL);
        }

        public VSOPResult_LBR(VSOPResult_XYZ result)
        {
            Variables_ELL = result.Variables_ELL;
            _referenceFrame = result.ReferenceFrame;
            Body = result.Body;
            Time = result.Time;
            Variables_LBR = Utility.XYZtoLBR(result.Variables_XYZ);
        }

        #region Elements

        /// <summary>
        /// Longitude (rad)
        /// </summary>
        public double l { get => Variables_LBR[0]; }

        /// <summary>
        /// Latitude (rad)
        /// </summary>
        public double b { get => Variables_LBR[1]; }

        /// <summary>
        /// Radius (au)
        /// </summary>
        public double r { get => Variables_LBR[2]; }

        /// <summary>
        /// Longitude velocity (rad/day)
        /// </summary>
        public double dl { get => Variables_LBR[3]; }

        /// <summary>
        /// Latitude velocity (rad/day)
        /// </summary>
        public double db { get => Variables_LBR[4]; }

        /// <summary>
        /// Radius velocity (au/day)
        /// </summary>
        public double dr { get => Variables_LBR[5]; }

        #endregion Elements

        public VSOPResult_XYZ ToXYZ()
        {
            return new VSOPResult_XYZ(this);
        }

        public VSOPResult_ELL ToELL()
        {
            return new VSOPResult_ELL(this);
        }

        public static explicit operator VSOPResult_XYZ(VSOPResult_LBR lbr) => new VSOPResult_XYZ(lbr);

        public static explicit operator VSOPResult_ELL(VSOPResult_LBR lbr) => new VSOPResult_ELL(lbr);
    }
}
