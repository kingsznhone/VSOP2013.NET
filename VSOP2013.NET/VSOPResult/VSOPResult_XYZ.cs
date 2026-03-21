namespace VSOP2013
{
    public sealed class VSOPResult_XYZ : VSOPResult
    {
        public override VSOPBody Body { get; }

        public override CoordinatesType CoordinatesType => CoordinatesType.Rectangular;

        private ReferenceFrame _referenceFrame;

        public override ReferenceFrame ReferenceFrame
        {
            get { return _referenceFrame; }
            set
            {
                if (_referenceFrame == ReferenceFrame.DynamicalJ2000 && value == ReferenceFrame.ICRSJ2000)
                {
                    Variables_XYZ = Utility.DynamicaltoICRS(Variables_XYZ);
                    _referenceFrame = value;
                }
                else if (_referenceFrame == ReferenceFrame.ICRSJ2000 && value == ReferenceFrame.DynamicalJ2000)
                {
                    Variables_XYZ = Utility.ICRStoDynamical(Variables_XYZ);
                    _referenceFrame = value;
                }
            }
        }

        public override VSOPTime Time { get; }

        public override double[] Variables_ELL { get; set; }

        public double[] Variables_XYZ { get; set; }

        public VSOPResult_XYZ(VSOPResult_LBR result)
        {
            Variables_ELL = result.Variables_ELL;
            _referenceFrame = result.ReferenceFrame;
            Body = result.Body;
            Time = result.Time;
            Variables_XYZ = Utility.LBRtoXYZ(result.Variables_LBR);
        }

        public VSOPResult_XYZ(VSOPResult_ELL result)
        {
            Variables_ELL = result.Variables_ELL;
            _referenceFrame = result.ReferenceFrame;
            Body = result.Body;
            Time = result.Time;
            Variables_XYZ = Utility.ELLtoXYZ(result.Body, result.Variables_ELL);
        }

        #region Elements

        /// <summary>
        /// Position x (au)
        /// </summary>
        public double x { get => Variables_XYZ[0]; }

        /// <summary>
        /// Position y (au)
        /// </summary>
        public double y { get => Variables_XYZ[1]; }

        /// <summary>
        /// Position z (au)
        /// </summary>
        public double z { get => Variables_XYZ[2]; }

        /// <summary>
        /// Velocity x (au/day)
        /// </summary>
        public double dx { get => Variables_XYZ[3]; }

        /// <summary>
        /// Velocity y (au/day)
        /// </summary>
        public double dy { get => Variables_XYZ[4]; }

        /// <summary>
        /// Velocity z (au/day)
        /// </summary>
        public double dz { get => Variables_XYZ[5]; }

        #endregion Elements

        public VSOPResult_LBR ToLBR()
        {
            return new VSOPResult_LBR(this);
        }

        public VSOPResult_ELL ToELL()
        {
            return new VSOPResult_ELL(this);
        }

        public static explicit operator VSOPResult_ELL(VSOPResult_XYZ xyz) => new VSOPResult_ELL(xyz);

        public static explicit operator VSOPResult_LBR(VSOPResult_XYZ xyz) => new VSOPResult_LBR(xyz);
    }
}
