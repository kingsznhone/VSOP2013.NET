using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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

    public enum VSOPFileName
    {
        VSOP2013p1 = 0,
        VSOP2013p2 = 1,
        VSOP2013p3 = 2,
        VSOP2013p4 = 3,
        VSOP2013p5 = 4,
        VSOP2013p6 = 5,
        VSOP2013p7 = 6,
        VSOP2013p8 = 7,
        VSOP2013p9 = 8
    }

    public struct PlanetEphemeris
    {
        //Elliptic   Elements 
        //a (au), lambda (radian), k, h, q, p
        //Dynamical Frame J2000'
        public double[] DynamicalELL;

        //Ecliptic   Heliocentric Coordinates
        //X,Y,Z (au)  X'',Y'',Z'' (au/d)
        //Dynamical Frame J2000'
        public double[] DynamicalXYZ;

        //Equatorial Heliocentric Coordinates:
        //X,Y,Z (au)  X'',Y'',Z'' (au/d)
        //ICRS Frame J2000
        public double[] ICRSXYZ;
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
        public int iv;
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
        public int iv;
        [Key(2)]
        public int it;
        [Key(3)]
        public Header header;
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
        public long rank;
        [Key(1)]
        [FieldOffset(8)]
        public double ss;
        [Key(2)]
        [FieldOffset(16)]
        public double cc;
        [Key(3)]
        [FieldOffset(24)]
        public double aa;
        [Key(4)]
        [FieldOffset(32)]
        public double bb;
    }
}
