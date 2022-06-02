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

    

    [Serializable]
    public struct PlanetTable
    {
        public VariableTable[] variables;
        public VSOPBody body;
    }

    [Serializable]
    public struct VariableTable
    {
        
        public PowerTable[] PowerTables;
        public VSOPBody Body;
        public int iv;
    }

    [Serializable]
    public struct PowerTable
    {
        public Header header;
        public Term[] Terms;
        public VSOPBody Body;
        
        public int iv;
        public int it;
    }

    [Serializable]
    public struct Header
    {

        /// <summary>
        /// body
        /// </summary>
        public int ip;
        /// <summary>
        /// variable
        /// </summary>
        public int iv;
        /// <summary>
        /// power
        /// </summary>
        public int it;
        /// <summary>
        /// sum of terms
        /// </summary>
        public int nt;
    }

    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct Term
    {
        [FieldOffset(0)]        
        public long rank;
        [FieldOffset(8)]
        public double ss;
        [FieldOffset(16)]
        public double cc;
        [FieldOffset(24)]
        public double aa;
        [FieldOffset(32)]
        public double bb;
    }
}
