using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOP2013.VSOPResult
{
    //Equatorial Heliocentric Coordinates:
    //X,Y,Z (au)  X'',Y'',Z'' (au/d)
    //ICRS Frame J2000
    public sealed class ICRSXYZ : ResultBase
    {
        /// <summary>
        /// Positions  X (au)
        /// </summary>
        public double X { get => data_ICRS[0]; }
        /// <summary>   
        /// Positions  Y (au)
        /// </summary>
        public double Y { get => data_ICRS[1]; }
        /// <summary>
        /// Positions  Z (au)
        /// </summary>
        public double Z { get => data_ICRS[2]; }
        /// <summary>
        /// Velocities X'(au/d)
        /// </summary>
        public double dX { get => data_ICRS[3]; }
        /// <summary>
        /// Velocities Y'(au/d)
        /// </summary>
        public double dY { get => data_ICRS[4]; }
        /// <summary>
        /// Velocities Z'(au/d)
        /// </summary>
        public double dZ { get => data_ICRS[5]; }

        private double[] data_ICRS;
        public ICRSXYZ(VSOPBody body, VSOPTime time, double[] variables_ell) : base(body, time, variables_ell)
        {
            data_ICRS = DynamicaltoICRS(ELLtoXYZ(body,data_ell));
        }

        public static explicit operator DynamicalELL(ICRSXYZ xyz)
        {
            return new DynamicalELL(xyz.Body, xyz.Time, xyz.data_ell);
        }

        public static explicit operator DynamicalXYZ(ICRSXYZ xyz)
        {
            return new DynamicalXYZ(xyz.Body, xyz.Time, xyz.data_ell);
        }
    }
}
