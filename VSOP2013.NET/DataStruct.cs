using MessagePack;
using System.Runtime.InteropServices;

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

    [MessagePackObject]
    [Serializable]
    public struct PlanetTable
    {
        [Key(0)]
        public VSOPBody body;

        [Key(1)]
        public VariableTable[] variables;
    }

    [MessagePackObject]
    [Serializable]
    public struct VariableTable
    {
        [Key(0)]
        public VSOPBody Body;

        [Key(1)]
        public VSOPVariable Variable;

        [Key(2)]
        public PowerTable[] PowerTables;
    }

    [MessagePackObject]
    [Serializable]
    public struct PowerTable
    {
        [Key(0)]
        public VSOPBody Body;

        [Key(1)]
        public VSOPVariable Variable;

        [Key(2)]
        public int Power;

        [Key(3)]
        public int TermsCount;

        [Key(4)]
        public Term[] Terms;      
    }

    [MessagePackObject]
    [Serializable]
    public struct Header
    {
        [Key(0)]
        public int ip;

        [Key(1)]
        public int iv;

        [Key(2)]
        public int it;

        [Key(3)]
        public int nt;
    }

    [MessagePackObject]
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Term
    {
        [Key(0)]
        [FieldOffset(0)]
        public double ss;

        [Key(1)]
        [FieldOffset(8)]
        public double cc;

        [Key(2)]
        [FieldOffset(16)]
        public double aa;

        [Key(3)]
        [FieldOffset(24)]
        public double bb;
    }
}