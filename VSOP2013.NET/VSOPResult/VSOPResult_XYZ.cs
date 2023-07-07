namespace VSOP2013
{
    public class VSOPResult_XYZ : VSOPResult
    {
        public override VSOPBody Body { get; }

        private CoordinatesReference _coordinatesReference;
        public override CoordinatesReference CoordinatesReference => _coordinatesReference;
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
                    _coordinatesReference = CoordinatesReference.EquatorialHeliocentric;
                }
                else if (_referenceFrame == ReferenceFrame.ICRSJ2000 && value == ReferenceFrame.DynamicalJ2000)
                {
                    Variables_XYZ = Utility.ICRStoDynamical(Variables_XYZ);
                    _referenceFrame = ReferenceFrame.DynamicalJ2000;
                    _coordinatesReference = CoordinatesReference.EclipticHeliocentric;
                }
            }
        }

        public override VSOPTime Time { get; }

        public override double[] Variables_ELL { get; set; }

        public double[] Variables_XYZ { get; set; }

        public VSOPResult_XYZ(VSOPResult_LBR result)
        {
            _coordinatesReference = result.CoordinatesReference;
            _referenceFrame = result.ReferenceFrame;
            Body = result.Body;
            Time = result.Time;
            Variables_XYZ = Utility.ELLtoXYZ(result.Body,result.Variables_ELL);
        }

        public VSOPResult_XYZ(VSOPResult_ELL result)
        {
            _coordinatesReference = result.CoordinatesReference;
            _referenceFrame = result.ReferenceFrame;
            Body = result.Body;
            Time = result.Time;
            Variables_XYZ = Utility.ELLtoXYZ(result.Body, result.Variables_ELL);
        }

        #region Elements

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

        #endregion
        public VSOPResult_LBR ToLBR()
        {
            return new VSOPResult_LBR(this);
        }
    }
}