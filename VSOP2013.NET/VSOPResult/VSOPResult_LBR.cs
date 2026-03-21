using System.Text.Json.Serialization;

namespace VSOP2013
{
    public sealed class VSOPResult_LBR : VSOPResult
    {
        public override CoordinatesType CoordinatesType => CoordinatesType.Spherical;

        /// <summary>longitude (rad)</summary>
        [JsonPropertyName("l")]
        public double l => _variables[0];

        /// <summary>latitude (rad)</summary>
        [JsonPropertyName("b")]
        public double b => _variables[1];

        /// <summary>radius (au)</summary>
        [JsonPropertyName("r")]
        public double r => _variables[2];

        /// <summary>longitude velocity (rad/day)</summary>
        [JsonPropertyName("dl")]
        public double dl => _variables[3];

        /// <summary>latitude velocity (rad/day)</summary>
        [JsonPropertyName("db")]
        public double db => _variables[4];

        /// <summary>radius velocity (au/day)</summary>
        [JsonPropertyName("dr")]
        public double dr => _variables[5];

        internal VSOPResult_LBR(VSOPBody body, VSOPTime time,
            double[] variables, ReferenceFrame frame)
            : base(body, time, variables, frame) { }

        public override VSOPResult_ELL ToELL()
        {
            double[] ell = Utility.LBRtoELL(Body, _variables);
            return new VSOPResult_ELL(Body, Time, ell, ReferenceFrame);
        }

        public override VSOPResult_XYZ ToXYZ()
        {
            double[] xyz = Utility.LBRtoXYZ(_variables);
            return new VSOPResult_XYZ(Body, Time, xyz, ReferenceFrame);
        }

        public override VSOPResult_LBR ToLBR() => this;

        /// <summary>
        /// Convert to target reference frame via XYZ rotation.
        /// Returns self if already in the target frame.
        /// </summary>
        public override VSOPResult_LBR ChangeFrame(ReferenceFrame targetFrame)
        {
            if (ReferenceFrame == targetFrame) return this;

            double[] xyz = Utility.LBRtoXYZ(_variables);
            double[] rotated = targetFrame == ReferenceFrame.ICRSJ2000
                ? Utility.DynamicaltoICRS(xyz)
                : Utility.ICRStoDynamical(xyz);
            double[] lbr = Utility.XYZtoLBR(rotated);
            return new VSOPResult_LBR(Body, Time, lbr, targetFrame);
        }
    }
}
