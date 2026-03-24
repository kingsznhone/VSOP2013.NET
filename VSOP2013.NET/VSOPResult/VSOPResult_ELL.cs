using System.Text.Json.Serialization;

namespace VSOP2013
{
    public sealed class VSOPResult_ELL : VSOPResult
    {
        public override CoordinatesType CoordinatesType => CoordinatesType.Elliptic;

        /// <summary>semi-major axis (au)</summary>
        [JsonPropertyName("a")]
        public double a => _variables[0];

        /// <summary>mean longitude (rad)</summary>
        [JsonPropertyName("l")]
        public double l => _variables[1];

        /// <summary>k = e*cos(pi) (dimensionless)</summary>
        [JsonPropertyName("k")]
        public double k => _variables[2];

        /// <summary>h = e*sin(pi) (dimensionless)</summary>
        [JsonPropertyName("h")]
        public double h => _variables[3];

        /// <summary>q = sin(i/2)*cos(omega) (dimensionless)</summary>
        [JsonPropertyName("q")]
        public double q => _variables[4];

        /// <summary>p = sin(i/2)*sin(omega) (dimensionless)</summary>
        [JsonPropertyName("p")]
        public double p => _variables[5];

        public VSOPResult_ELL(VSOPBody body, VSOPTime time,
            double[] variables, ReferenceFrame frame)
            : base(body, time, variables, frame) { }

        public override VSOPResult_ELL ToELL() => this;

        public override VSOPResult_XYZ ToXYZ()
        {
            double[] xyz = Utility.ELLtoXYZ(Body, _variables);
            return new VSOPResult_XYZ(Body, Time, xyz, ReferenceFrame);
        }

        public override VSOPResult_LBR ToLBR()
        {
            double[] lbr = Utility.ELLtoLBR(Body, _variables);
            return new VSOPResult_LBR(Body, Time, lbr, ReferenceFrame);
        }

        /// <summary>
        /// Convert to target reference frame via XYZ rotation.
        /// Returns self if already in the target frame.
        /// </summary>
        public override VSOPResult_ELL ChangeFrame(ReferenceFrame targetFrame)
        {
            if (ReferenceFrame == targetFrame) return this;

            double[] xyz = Utility.ELLtoXYZ(Body, _variables);
            double[] rotated = targetFrame == ReferenceFrame.ICRSJ2000
                ? Utility.DynamicaltoICRS(xyz)
                : Utility.ICRStoDynamical(xyz);
            double[] ell = Utility.XYZtoELL(Body, rotated);
            return new VSOPResult_ELL(Body, Time, ell, targetFrame);
        }
    }
}
