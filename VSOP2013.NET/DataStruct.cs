using System.Runtime.InteropServices;
using MemoryPack;

namespace VSOP2013
{
    public enum VSOPBody
    {
        MERCURY = 0,
        VENUS = 1,
        EMB = 2,
        MARS = 3,
        JUPITER = 4,
        SATURN = 5,
        URANUS = 6,
        NEPTUNE = 7,
        PLUTO = 8
    }

    public enum VSOPVariable
    {
        A = 0,
        L = 1,
        K = 2,
        H = 3,
        Q = 4,
        P = 5,
    }

    [MemoryPackable]
    [Serializable]
    public partial struct PlanetTable
    {
        public VSOPBody body;
        public VariableTable[] variables;
    }

    [MemoryPackable]
    [Serializable]
    public partial struct VariableTable
    {
        public VSOPBody Body;
        public VSOPVariable Variable;
        public PowerTable[] PowerTables;
    }

    [MemoryPackable]
    [Serializable]
    public partial struct PowerTable
    {
        public VSOPBody Body;
        public VSOPVariable Variable;
        public int Power;
        public int TermsCount;
        public Term[] Terms;
    }

    [MemoryPackable]
    [Serializable]
    public partial struct Header
    {
        public int ip;
        public int iv;
        public int it;
        public int nt;
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
