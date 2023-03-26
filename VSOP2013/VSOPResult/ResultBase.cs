using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VSOP2013.VSOPResult
{
    public abstract class ResultBase
    {
        protected readonly double[] gmp = {4.9125474514508118699e-11d,
                        7.2434524861627027000e-10d,
                        8.9970116036316091182e-10d,
                        9.5495351057792580598e-11d,
                        2.8253458420837780000e-07d,
                        8.4597151856806587398e-08d,
                        1.2920249167819693900e-08d,
                        1.5243589007842762800e-08d,
                        2.1886997654259696800e-12d};

        protected readonly double gmsol = 2.9591220836841438269e-04d;

        public VSOPBody Body { get; private set; }
        public VSOPTime Time { get; private set; }

        //Elliptic   Elements 
        //a (au), lambda (radian), k, h, q, p
        //Dynamical Frame J2000'
        protected double[] data_ell;

        public ResultBase(VSOPBody body, VSOPTime time, double[] variables)
        {
            Body = body;
            Time = time;
            data_ell = variables;
        }

        /// <summary>
        /// This is kind of magic that I can't undersdand
        /// translate from FORTRAN code
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="ELL">Elliptic Elements</param>
        /// <returns>Ecliptic Heliocentric Coordinates</returns>
        protected double[] ELLtoXYZ(VSOPBody body, double[] ELL)
        {

            double[] xyz;
            Complex z, z1, z2, z3, zto, zteta;
            double rgm, xfi, xki;
            double u, ex, ex2, ex3, gl, gm, e, dl, rsa;
            double xm, xr, xms, xmc, xn;

            //Initialization
            rgm = Math.Sqrt(gmp[(int)body] + gmsol);
            xyz = new double[6];

            //Computation
            xfi = Math.Sqrt(1.0d - (ELL[2] * ELL[2]) - (ELL[3] * ELL[3]));
            xki = Math.Sqrt(1.0d - (ELL[4] * ELL[4]) - (ELL[5] * ELL[5]));
            u = 1.0d / (1.0d + xfi);
            z = new Complex(ELL[2], ELL[3]);
            ex = z.Magnitude;
            ex2 = ex * ex;
            ex3 = ex2 * ex;
            z1 = Complex.Conjugate(z);
            gl = ELL[1] % (2 * Math.PI);
            gm = gl - Math.Atan2(ELL[3], ELL[2]);
            e = gl + (ex - 0.125d * ex3) * Math.Sin(gm)
                + 0.5d * ex2 * Math.Sin(2.0d * gm)
                + 0.375d * ex3 * Math.Sin(3.0d * gm);

            z2 = new Complex(0d, e);
            zteta = Complex.Exp(z2);
            while (true)
            {
                z3 = z1 * zteta;
                dl = gl - e + z3.Imaginary;
                rsa = 1.0d - z3.Real;
                e = e + dl / rsa;
                if (Math.Abs(dl) < Math.Pow(10, -15)) break;
            }

            z1 = u * z * z3.Imaginary;
            z2 = new Complex(z1.Imaginary, -z1.Real);
            zto = (-z + zteta + z2) / rsa;
            xm = ELL[5] * zto.Real - ELL[4] * zto.Imaginary;
            xr = ELL[0] * rsa;

            xyz[0] = xr * (zto.Real - 2.0d * ELL[5] * xm);
            xyz[1] = xr * (zto.Imaginary + 2.0d * ELL[4] * xm);
            xyz[2] = -2.0d * xr * xki * xm;
            xms = ELL[0] * (ELL[3] + zto.Imaginary) / xfi;
            xmc = ELL[0] * (ELL[2] + zto.Real) / xfi;

            xn = rgm / Math.Pow(ELL[0], 1.5d);
            xyz[3] = xn * ((2.0d * ELL[5] * ELL[5] - 1.0d) * xms + 2.0d * ELL[5] * ELL[4] * xmc);
            xyz[4] = xn * ((1.0d - 2.0d * ELL[4] * ELL[4]) * xmc - 2.0d * ELL[5] * ELL[4] * xms);
            xyz[5] = 2.0d * xn * xki * (ELL[5] * xms + ELL[4] * xmc);

            return xyz;
        }


        /// <summary>
        /// Another magic function
        /// </summary>
        /// <param name="xyz">Ecliptic Heliocentric Coordinates - Dynamical Frame J2000</param>
        /// <returns>Equatorial Heliocentric Coordinates - ICRS Frame J2000</returns>
        protected double[] DynamicaltoICRS(double[] xyz)
        {
            double[] icrs = new double[6];
            //Rotation Matrix
            double[,] Rot = new double[3, 3];
            double pi = Math.PI;
            double dgrad = pi / 180.0d;
            double sdrad = pi / 180.0d / 3600.0d;
            double eps = (23.0d + 26.0d / 60.0d + 21.411360d / 3600.0d) * dgrad;
            double phi = -0.051880d * sdrad;

            double seps, ceps, sphi, cphi;
            (seps, ceps) = Math.SinCos(eps);
            (sphi, cphi) = Math.SinCos(phi);

            Rot[0, 0] = cphi;
            Rot[0, 1] = -sphi * ceps;
            Rot[0, 2] = sphi * seps;
            Rot[1, 0] = sphi;
            Rot[1, 1] = cphi * ceps;
            Rot[1, 2] = -cphi * seps;
            Rot[2, 0] = 0.0d;
            Rot[2, 1] = seps;
            Rot[2, 2] = ceps;

            //Computation 
            for (int i = 0; i < 3; i++)
            {
                icrs[i] = 0.0d;
                icrs[i + 3] = 0.0d;
                for (int j = 0; j < 3; j++)
                {
                    icrs[i] = icrs[i] + Rot[i, j] * xyz[j];
                    icrs[i + 3] = icrs[i + 3] + Rot[i, j] * xyz[j + 3];
                }
            }
            return icrs;
        }

    }
}
