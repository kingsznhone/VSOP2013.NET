using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VSOP2013.VSOPResult
{
    /// <summary>
    /// Ecliptic   Heliocentric Coordinates
    /// X,Y,Z (au)  X'',Y'',Z'' (au/d)
    /// Dynamical Frame J2000'
    /// </summary>
    public sealed class DynamicalXYZ : ResultBase
    {
        /// <summary>
        /// Positions  X (au)
        /// </summary>
        public double X { get => data_xyz[0]; }
        /// <summary>
        /// Positions  Y (au)
        /// </summary>
        public double Y { get => data_xyz[1]; }
        /// <summary>
        /// Positions  Z (au)
        /// </summary>
        public double Z { get => data_xyz[2]; }
        /// <summary>
        /// Velocities X'(au/d)
        /// </summary>
        public double dX { get => data_xyz[3]; }
        /// <summary>
        /// Velocities Y'(au/d)
        /// </summary>
        public double dY { get => data_xyz[4]; }
        /// <summary>
        /// Velocities Z'(au/d)
        /// </summary>
        public double dZ { get => data_xyz[5]; }

        private double[] data_xyz { get; set; }

        public DynamicalXYZ(VSOPBody body, VSOPTime time, double[] variables_ell) : base(body, time, variables_ell)
        {
            data_xyz = ELLtoXYZ(body,data_ell);
        }

        public static explicit operator DynamicalELL(DynamicalXYZ xyz)
        {
            return new DynamicalELL(xyz.Body, xyz.Time, xyz.data_ell);
        }

        public static explicit operator ICRSXYZ(DynamicalXYZ xyz)
        {
            return new ICRSXYZ(xyz.Body, xyz.Time, xyz.data_ell);
        }

    }
}
