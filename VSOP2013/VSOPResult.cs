using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VSOP2013
{
    public class VSOPResult
    {
        //Masses system
        double[] gmp = {4.9125474514508118699e-11d,
                        7.2434524861627027000e-10d,
                        8.9970116036316091182e-10d,
                        9.5495351057792580598e-11d,
                        2.8253458420837780000e-07d,
                        8.4597151856806587398e-08d,
                        1.2920249167819693900e-08d,
                        1.5243589007842762800e-08d,
                        2.1886997654259696800e-12d};

        double gmsol = 2.9591220836841438269e-04d;

        public VSOPBody Body { get; }

        public VSOPTime Time { get; }

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


        public VSOPResult(VSOPBody body, VSOPTime time, double[] variables)
        {
            Body = body;
            Time = time;
            DynamicalELL = variables;
            DynamicalXYZ = ELLtoXYZ((int)body, DynamicalELL);
            ICRSXYZ = DynamicaltoICRS(DynamicalXYZ);
        }

        /// <summary>
        /// This is kind of magic that I can't undersdand
        /// translate from FORTRAN code
        /// </summary>
        /// <param name="ip">planet</param>
        /// <param name="ELL">Elliptic Elements</param>
        /// <returns>Ecliptic Heliocentric Coordinates</returns>
        private double[] ELLtoXYZ(int ip, double[] ELL)
        {

            double[] w;
            Complex z, z1, z2, z3, zto, zteta;
            double rgm, xa, xl, xk, xh, xq, xp, xfi, xki;
            double u, ex, ex2, ex3, gl, gm, e, dl, rsa;
            double xcw, xsw, xm, xr, xms, xmc, xn;

            //Initialization
            rgm = Math.Sqrt(gmp[ip] + gmsol);
            w = new double[6];
            xa = ELL[0];
            xl = ELL[1];
            xk = ELL[2];
            xh = ELL[3];
            xq = ELL[4];
            xp = ELL[5];
            //Computation
            xfi = Math.Sqrt(1.0d - (xk * xk) - (xh * xh));
            xki = Math.Sqrt(1.0d - (xq * xq) - (xp * xp));
            u = 1.0d / (1.0d + xfi);
            z = new Complex(xk, xh);
            ex = z.Magnitude;
            ex2 = ex * ex;
            ex3 = ex2 * ex;
            z1 = Complex.Conjugate(z);
            gl = xl % (2 * Math.PI);
            gm = gl - Math.Atan2(xh, xk);
            e = gl + (ex - 0.125d * ex3) * Math.Sin(gm)
                + 0.5d * ex2 * Math.Sin(2.0d * gm)
                + 0.375d * ex3 * Math.Sin(3.0d * gm);

            while (true)
            {
                z2 = new Complex(0d, e);
                zteta = Complex.Exp(z2);
                z3 = z1 * zteta;
                dl = gl - e + z3.Imaginary;
                rsa = 1.0d - z3.Real;
                e = e + dl / rsa;
                if (Math.Abs(dl) < Math.Pow(10, -15)) break;
            }

            z1 = u * z * z3.Imaginary;
            z2 = new Complex(z1.Imaginary, -z1.Real);
            zto = (-z + zteta + z2) / rsa;
            xcw = zto.Real;
            xsw = zto.Imaginary;
            xm = xp * xcw - xq * xsw;
            xr = xa * rsa;

            w[0] = xr * (xcw - 2.0d * xp * xm);
            w[1] = xr * (xsw + 2.0d * xq * xm);
            w[2] = -2.0d * xr * xki * xm;
            xms = xa * (xh + xsw) / xfi;
            xmc = xa * (xk + xcw) / xfi;

            xn = rgm / Math.Pow(xa, 1.5d);
            w[3] = xn * ((2.0d * xp * xp - 1.0d) * xms + 2.0d * xp * xq * xmc);
            w[4] = xn * ((1.0d - 2.0d * xq * xq) * xmc - 2.0d * xp * xq * xms);
            w[5] = 2.0d * xn * xki * (xp * xms + xq * xmc);

            return w;
        }

        /// <summary>
        /// Another magic function
        /// </summary>
        /// <param name="w">Ecliptic Heliocentric Coordinates - Dynamical Frame J2000</param>
        /// <returns>Equatorial Heliocentric Coordinates - ICRS Frame J2000</returns>
        public double[] DynamicaltoICRS(double[] w)
        {
            double[] w2 = new double[6];
            //Rotation Matrix
            double[,] rot = new double[3, 3];
            double pi = Math.PI;
            double dgrad = pi / 180.0d;
            double sdrad = pi / 180.0d / 3600.0d;
            double eps = (23.0d + 26.0d / 60.0d + 21.411360d / 3600.0d) * dgrad;
            double phi = -0.051880d * sdrad;
            double ceps = Math.Cos(eps);
            double seps = Math.Sin(eps);
            double cphi = Math.Cos(phi);
            double sphi = Math.Sin(phi);
            rot[0, 0] = cphi;
            rot[0, 1] = -sphi * ceps;
            rot[0, 2] = sphi * seps;
            rot[1, 0] = sphi;
            rot[1, 1] = cphi * ceps;
            rot[1, 2] = -cphi * seps;
            rot[2, 0] = 0.0d;
            rot[2, 1] = seps;
            rot[2, 2] = ceps;

            //Computation
            for (int i = 0; i < 3; i++)
            {
                w2[i] = 0.0d;
                w2[i + 3] = 0.0d;
                for (int j = 0; j < 3; j++)
                {
                    w2[i] = w2[i] + rot[i, j] * w[j];
                    w2[i + 3] = w2[i + 3] + rot[i, j] * w[j + 3];
                }
            }
            return w2;
        }

    }
}
