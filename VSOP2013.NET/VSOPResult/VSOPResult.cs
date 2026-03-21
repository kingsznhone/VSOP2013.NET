using System.Text.Json.Serialization;

namespace VSOP2013
{
    [JsonConverter(typeof(JsonStringEnumConverter<CoordinatesType>))]
    public enum CoordinatesType
    {
        Elliptic = 0,
        Rectangular = 1,
        Spherical = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter<ReferenceFrame>))]
    public enum ReferenceFrame
    {
        /// <summary>
        /// Ecliptic Heliocentric Dynamical Frame
        /// </summary>
        DynamicalJ2000 = 0,

        /// <summary>
        /// Equatorial Heliocentric ICRS Frame
        /// </summary>
        ICRSJ2000 = 2
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(VSOPResult_ELL), "ELL")]
    [JsonDerivedType(typeof(VSOPResult_XYZ), "XYZ")]
    [JsonDerivedType(typeof(VSOPResult_LBR), "LBR")]
    public abstract class VSOPResult
    {
        public VSOPBody Body { get; }
        public VSOPTime Time { get; }
        public abstract CoordinatesType CoordinatesType { get; }
        public ReferenceFrame ReferenceFrame { get; }

        protected readonly double[] _variables;

        /// <summary>
        /// Raw 6-element coordinate array (read-only view).
        /// </summary>
        [JsonIgnore]
        public ReadOnlySpan<double> Variables => _variables;

        protected VSOPResult(VSOPBody body, VSOPTime time,
            double[] variables, ReferenceFrame frame)
        {
            Body = body;
            Time = time;
            _variables = variables;
            ReferenceFrame = frame;
        }

        public abstract VSOPResult_ELL ToELL();

        public abstract VSOPResult_XYZ ToXYZ();

        public abstract VSOPResult_LBR ToLBR();

        public abstract VSOPResult ChangeFrame(ReferenceFrame targetFrame);
    }
}
