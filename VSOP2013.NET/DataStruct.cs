using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using MemoryPack;

namespace VSOP2013
{
    [JsonConverter(typeof(JsonStringEnumConverter<VSOPBody>))]
    public enum VSOPBody
    {
        SUN = 0,
        MERCURY = 1,
        VENUS = 2,
        EMB = 3,
        MARS = 4,
        JUPITER = 5,
        SATURN = 6,
        URANUS = 7,
        NEPTUNE = 8,
        PLUTO = 9
    }

    public enum VSOPVariable
    {
        A = 1,
        L = 2,
        K = 3,
        H = 4,
        Q = 5,
        P = 6,
    }

    [MemoryPackable]
    [Serializable]
    public partial class PlanetTable
    {
        public VSOPBody Body;
        public Dictionary<VSOPVariable, VariableTable> variables;
    }

    [MemoryPackable]
    [Serializable]
    public partial class VariableTable
    {
        public VSOPBody Body;
        public VSOPVariable Variable;
        public PowerTable[] PowerTables;
    }

    [MemoryPackable]
    [Serializable]
    public partial class PowerTable
    {
        public VSOPBody Body;
        public VSOPVariable Variable;
        public int Power;
        public int TermsCount;
        public Term[] Terms;
    }

    public partial struct Header
    {
        public VSOPBody Body;
        public VSOPVariable Variable;
        public int Power;
        public int TermsCount;
    }

    [MemoryPackable]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public partial struct Term
    {
        [FieldOffset(0)]
        public double ss;

        [FieldOffset(8)]
        public double cc;

        [FieldOffset(16)]
        public double aa;

        [FieldOffset(24)]
        public double bb;
    }
}
