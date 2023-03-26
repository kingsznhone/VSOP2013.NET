using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOP2013.VSOPResult
{
    /// <summary>
    /// Elliptic   Elements 
    /// a (au), lambda (radian), k, h, q, p
    /// Dynamical Frame J2000'
    /// </summary>
    public sealed class DynamicalELL : ResultBase
    {
        /// <summary>
        /// a = semi-major axis (au)
        /// </summary>
        public double A { get => data_ell[0]; }
        /// <summary>
        /// l = mean longitude (rd)
        /// </summary>
        public double L { get => data_ell[1]; }
        /// <summary>
        /// k = e*cos(pi) (rd)
        /// </summary>
        public double K { get => data_ell[2]; }
        /// <summary>
        /// h = e*sin(pi) (rd)
        /// </summary>
        public double H { get => data_ell[3]; }
        /// <summary>
        /// q = sin(i/2)*cos(omega) (rd)
        /// </summary>
        public double Q { get => data_ell[4]; }
        /// <summary>
        /// p = sin(i/2)*sin(omega) (rd)
        /// </summary>
        public double P { get => data_ell[5]; }

        public DynamicalELL(VSOPBody body, VSOPTime time, double[] variables_ell) : base(body, time, variables_ell) { }


        public static explicit operator DynamicalXYZ(DynamicalELL ELL)
        {
            return new DynamicalXYZ(ELL.Body, ELL.Time, ELL.data_ell);
        }

        public static explicit operator ICRSXYZ(DynamicalELL ELL)
        {
            return new ICRSXYZ(ELL.Body, ELL.Time, ELL.data_ell);
        }

    }
}
