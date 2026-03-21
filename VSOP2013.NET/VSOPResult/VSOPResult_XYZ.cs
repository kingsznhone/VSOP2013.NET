using System.Text.Json.Serialization;

namespace VSOP2013
{
    public sealed class VSOPResult_XYZ : VSOPResult
    {
        public override CoordinatesType CoordinatesType => CoordinatesType.Rectangular;

        /// <summary>position x (au)</summary>
        [JsonPropertyName("x")]
        public double x => _variables[0];

        /// <summary>position y (au)</summary>
        [JsonPropertyName("y")]
        public double y => _variables[1];

        /// <summary>position z (au)</summary>
        [JsonPropertyName("z")]
        public double z => _variables[2];

        /// <summary>velocity x (au/day)</summary>
        [JsonPropertyName("dx")]
        public double dx => _variables[3];

        /// <summary>velocity y (au/day)</summary>
        [JsonPropertyName("dy")]
        public double dy => _variables[4];

        /// <summary>velocity z (au/day)</summary>
        [JsonPropertyName("dz")]
        public double dz => _variables[5];

        internal VSOPResult_XYZ(VSOPBody body, VSOPTime time,
            double[] variables, ReferenceFrame frame)
            : base(body, time, variables, frame) { }

        public override VSOPResult_ELL ToELL()
        {
            double[] ell = Utility.XYZtoELL(Body, _variables);
            return new VSOPResult_ELL(Body, Time, ell, ReferenceFrame);
        }

        public override VSOPResult_XYZ ToXYZ() => this;

        public override VSOPResult_LBR ToLBR()
        {
            double[] lbr = Utility.XYZtoLBR(_variables);
            return new VSOPResult_LBR(Body, Time, lbr, ReferenceFrame);
        }

        /// <summary>
        /// Convert to target reference frame. Direct rotation, no intermediate conversion.
        /// Returns self if already in the target frame.
        /// </summary>
        public override VSOPResult_XYZ ChangeFrame(ReferenceFrame targetFrame)
        {
            if (ReferenceFrame == targetFrame) return this;

            double[] rotated = targetFrame == ReferenceFrame.ICRSJ2000
                ? Utility.DynamicaltoICRS(_variables)
                : Utility.ICRStoDynamical(_variables);
            return new VSOPResult_XYZ(Body, Time, rotated, targetFrame);
        }
    }
}
