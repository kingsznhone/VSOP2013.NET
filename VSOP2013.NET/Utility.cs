using System.Numerics;
using System.Runtime.Intrinsics;

namespace VSOP2013
{
    public static class Utility
    {
        public static readonly double[] gmp = {4.9125474514508118699e-11d,
                        7.2434524861627027000e-10d,
                        8.9970116036316091182e-10d,
                        9.5495351057792580598e-11d,
                        2.8253458420837780000e-07d,
                        8.4597151856806587398e-08d,
                        1.2920249167819693900e-08d,
                        1.5243589007842762800e-08d,
                        2.1886997654259696800e-12d};

        public static readonly double gmsol = 2.9591220836841438269e-04d;

        public static double[,] MultiplyMatrix(double[,] A, double[,] B)
        {
            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);

            if (cA != rB)
            {
                Console.WriteLine("Matrixes can't be multiplied!!");
                return null;
            }
            else
            {
                double temp;
                double[,] kHasil = new double[rA, cB];

                for (int i = 0; i < rA; i++)
                {
                    for (int j = 0; j < cB; j++)
                    {
                        temp = 0;
                        for (int k = 0; k < cA; k++)
                        {
                            temp += A[i, k] * B[k, j];
                        }
                        kHasil[i, j] = temp;
                    }
                }

                return kHasil;
            }
        }

        /// <summary>
        /// Convert cardinal coordinate to spherical coordinate
        /// </summary>
        /// <param name="xyz">x,y,z,dx,dy,dz</param>
        /// <returns>l,b,r,dl,db,dr</returns>
        public static double[] XYZtoLBR(double[] xyz)
        {
            Span<double> lbr = stackalloc double[6];

            double x = xyz[0];
            double y = xyz[1];
            double z = xyz[2];
            double dx = xyz[3];
            double dy = xyz[4];
            double dz = xyz[5];
            double l, b, r, dl, db, dr;

            //r = sqrt(x^2+y^2+z^2)
            r = Math.Sqrt(x * x + y * y + z * z);

            //L = sgn(y) * arccos(x/sqrt(x^2+y^2))
            if (y >= 0)
            {
                l = Math.Acos(x / Math.Sqrt(x * x + y * y));
            }
            else
            {
                l = -Math.Acos(x / Math.Sqrt(x * x + y * y));
            }

            //θ = arccos(z/r)
            //b = pi/2 - θ.
            //Sin(θ) = Cos(b), Cos(θ) = Sin(b)
            b = Math.Asin(z / r);

#if NET7_0

            #region vector matrix mul

            if (Vector256.IsHardwareAccelerated)
            {
                Vector256<double> v1 = Vector256.Create(x / r, y / r, z / r, 0);
                Vector256<double> v2 = Vector256.Create(x * z / (r * r * Math.Sqrt(x * x + y * y)), y * z / (r * r * Math.Sqrt(x * x + y * y)), -(x * x + y * y) / (r * r * Math.Sqrt(x * x + y * y)), 0);
                Vector256<double> v3 = Vector256.Create(-y / (x * x + y * y), x / (x * x + y * y), 0, 0);
                Vector256<double> vv = Vector256.Create(dx, dy, dz, 0);

                lbr[0] = l;
                lbr[1] = b;
                lbr[2] = r;
                lbr[3] = Vector256.Sum(vv * v1);
                lbr[4] = Vector256.Sum(vv * v2);
                lbr[5] = Vector256.Sum(vv * v3);
                return lbr.ToArray();
            }

            #endregion vector matrix mul

#endif

            // Inverse Jacobian matrix  From  Caridnal to Sperical
            //https://en.wikipedia.org/wiki/Spherical_coordinate_system#Integration_and_differentiation_in_spherical_coordinates
            double[,] J_1 = {
                           {x/r                          , y/r                         , z/r                                 },
                           {x*z/(r*r*Math.Sqrt(x*x+y*y)) , y*z/(r*r*Math.Sqrt(x*x+y*y)),-(x*x+y*y)/(r*r*Math.Sqrt(x*x+y*y))  },
                           {-y/(x*x+y*y)                 , x/(x*x+y*y)                 , 0                                   }};
            double[,] Velocity = { { dx }, { dy }, { dz } };
            var C = MultiplyMatrix(J_1, Velocity);
            dl = C[2, 0];
            db = C[1, 0];
            dr = C[0, 0];

            //modulo r into [0,Tau)
            l -= Math.Floor(l / Math.Tau) * Math.Tau;
            if (l < 0)
                l += Math.Tau;

            lbr[0] = l;
            lbr[1] = b;
            lbr[2] = r;
            lbr[3] = dl;
            lbr[4] = -db;
            lbr[5] = dr;
            return lbr.ToArray();
        }

        /// <summary>
        /// convert spherical coordinate to cardinal coordinate
        /// </summary>
        /// <param name="lbr">l,b,r,dl,db,dr</param>
        /// <returns>x,y,z,dx,dy,dz</returns>
        public static double[] LBRtoXYZ(double[] lbr)
        {
            Span<double> xyz = stackalloc double[6];
            double x, y, z, dx, dy, dz;
            double l, b, r, dl, db, dr;

            l = lbr[0];
            b = lbr[1];
            r = lbr[2];
            dl = lbr[3];
            db = lbr[4];
            dr = lbr[5];

            x = r * Math.Cos(b) * Math.Cos(l);
            y = r * Math.Cos(b) * Math.Sin(l);
            z = r * Math.Sin(b);

#if NET7_0

            #region vector matrix mul

            if (Vector256.IsHardwareAccelerated)
            {
                Vector256<double> m1 = Vector256.Create(Math.Cos(b) * Math.Cos(l), r * Math.Sin(b) * Math.Cos(l), -r * Math.Cos(b) * Math.Sin(l), 0);
                Vector256<double> m2 = Vector256.Create(Math.Cos(b) * Math.Sin(l), r * Math.Sin(b) * Math.Sin(l), r * Math.Cos(b) * Math.Cos(l), 0);
                Vector256<double> m3 = Vector256.Create(Math.Sin(b), -r * Math.Cos(b), 0, 0);
                Vector256<double> vv = Vector256.Create(dr, db, dl, 0);

                xyz[0] = x;
                xyz[1] = y;
                xyz[2] = z;
                xyz[3] = Vector256.Sum(vv * m1);
                xyz[4] = Vector256.Sum(vv * m2);
                xyz[5] = Vector256.Sum(vv * m3);
                return xyz.ToArray();
            }

            #endregion vector matrix mul

#endif
            // Jacobian matrix From Sperical to Caridnal
            //https://en.wikipedia.org/wiki/Spherical_coordinate_system#Integration_and_differentiation_in_spherical_coordinates
            double[,] J = {
                           { Math.Cos(b) * Math.Cos(l),  r * Math.Sin(b) * Math.Cos(l), -r * Math.Cos(b) * Math.Sin(l)  },
                           { Math.Cos(b) * Math.Sin(l),  r * Math.Sin(b) * Math.Sin(l),  r * Math.Cos(b) * Math.Cos(l)  },
                           { Math.Sin(b),               -r * Math.Cos(b),                0                              }};
            double[,] Velocity = { { dr }, { db }, { dl } };
            var C = MultiplyMatrix(J, Velocity);
            dx = C[0, 0];
            dy = C[1, 0];
            dz = C[2, 0];

            xyz[0] = x;
            xyz[1] = y;
            xyz[2] = z;
            xyz[3] = dx;
            xyz[4] = dy;
            xyz[5] = -dz;
            return xyz.ToArray();
        }

        /// <summary>
        /// Convert Elliptic coordinate to cardinal coordinate.
        /// This is kind of magic that I will never undersdand.
        /// Directly translate from VSOP2013.f.
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="ell">Elliptic Elements: a,l,k,h,q,p </param>
        /// <returns>Cardinal Heliocentric Coordinates</returns>
        public static double[] ELLtoXYZ(VSOPBody body, double[] ell)
        {
            Span<double> xyz = stackalloc double[6];
            double a, l, k, h, q, p;
            a = ell[0];
            l = ell[1];
            k = ell[2];
            h = ell[3];
            q = ell[4];
            p = ell[5];
            Complex z, z1, z2, z3, zto, zteta;
            double rgm, xfi, xki;
            double u, ex, ex2, ex3, gl, gm, e, dl, rsa;
            double xm, xr, xms, xmc, xn;

            //Initialization
            rgm = Math.Sqrt(gmp[(int)body] + gmsol);

            //Computation
            xfi = Math.Sqrt(1.0d - (k * k) - (h * h));
            xki = Math.Sqrt(1.0d - (q * q) - (p * p));
            u = 1.0d / (1.0d + xfi);
            z = new Complex(k, h);
            ex = z.Magnitude;
            ex2 = ex * ex;
            ex3 = ex2 * ex;
            z1 = Complex.Conjugate(z);
            gl = ell[1] % (Math.Tau);
            gm = gl - Math.Atan2(h, k);
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
                e += dl / rsa;
                if (Math.Abs(dl) < Math.Pow(10, -15)) break;
            }

            z1 = u * z * z3.Imaginary;
            z2 = new Complex(z1.Imaginary, -z1.Real);
            zto = (-z + zteta + z2) / rsa;
            xm = p * zto.Real - q * zto.Imaginary;
            xr = a * rsa;

            xyz[0] = xr * (zto.Real - 2.0d * p * xm);
            xyz[1] = xr * (zto.Imaginary + 2.0d * q * xm);
            xyz[2] = -2.0d * xr * xki * xm;
            xms = a * (h + zto.Imaginary) / xfi;
            xmc = a * (k + zto.Real) / xfi;

            xn = rgm / Math.Pow(a, 1.5d);
            xyz[3] = xn * (((2.0d * p * p) - 1.0d) * xms + (2.0d * p * q * xmc));
            xyz[4] = xn * ((1.0d - (2.0d * q * q)) * xmc - (2.0d * p * q * xms));
            xyz[5] = xn * 2.0d * xki * (p * xms + q * xmc);

            return xyz.ToArray();
        }

        /// <summary>
        /// Convert Elliptic coordinate to spherical coordinate.
        /// </summary>
        /// <param name="body">planet</param>
        /// <returns>Spherical Heliocentric Coordinates</returns>
        public static double[] ELLtoLBR(VSOPBody body, double[] ell)
        {
            double[] xyz = ELLtoXYZ(body, ell);
            return XYZtoLBR(xyz);
        }

        /// <summary>
        /// Convert innertial frame from dynamical to ICRS.
        /// </summary>
        /// <param name="dynamical">Ecliptic Heliocentric Coordinates - Dynamical Frame J2000</param>
        /// <returns>Equatorial Heliocentric Coordinates - ICRS Frame J2000</returns>
        public static double[] DynamicaltoICRS(double[] dynamical)
        {
            Span<double> icrs = stackalloc double[6];

            double dgrad = Math.PI / 180.0d;
            double sdrad = Math.PI / 180.0d / 3600.0d;
            double eps = (23.0d + 26.0d / 60.0d + 21.411360d / 3600.0d) * dgrad;
            double phi = -0.051880d * sdrad;

            double Seps, Ceps, Sphi, Cphi;
            Seps = Math.Sin(eps);
            Ceps = Math.Cos(eps);
            Sphi = Math.Sin(phi);
            Cphi = Math.Cos(phi);

#if NET7_0

            #region vector matrix mul

            if (Vector256.IsHardwareAccelerated)
            {
                Vector256<double> m1 = Vector256.Create(Cphi, -Sphi * Ceps, Sphi * Seps, 0);
                Vector256<double> m2 = Vector256.Create(Sphi, Cphi * Ceps, -Cphi * Seps, 0);
                Vector256<double> m3 = Vector256.Create(0, Seps, Ceps, 0);
                Vector256<double> vv = Vector256.Create(dynamical[0], dynamical[1], dynamical[2], 0);
                Vector256<double> vdv = Vector256.Create(dynamical[3], dynamical[4], dynamical[5], 0);

                icrs[0] = Vector256.Sum(vv * m1);
                icrs[1] = Vector256.Sum(vv * m2);
                icrs[2] = Vector256.Sum(vv * m3);

                icrs[3] = Vector256.Sum(vdv * m1);
                icrs[4] = Vector256.Sum(vdv * m2);
                icrs[5] = Vector256.Sum(vdv * m3);
                return icrs.ToArray();
            }

            #endregion vector matrix mul

#endif

            //Rotation Matrix
            double[,] R = new double[,] { {Cphi, -Sphi*Ceps,  Sphi*Seps },
                                          {Sphi,  Cphi*Ceps, -Cphi*Seps },
                                          {0,     Seps,       Ceps      }};

            // Vector for cardinal coordnate element
            double[,] A = new double[,] { {dynamical[0]},
                                          {dynamical[1]},
                                          {dynamical[2]}};

            double[,] C = MultiplyMatrix(R, A);

            icrs[0] = C[0, 0];
            icrs[1] = C[1, 0];
            icrs[2] = C[2, 0];

            A = new double[,] { {dynamical[3]},
                                {dynamical[4]},
                                {dynamical[5]}};

            C = MultiplyMatrix(R, A);
            icrs[3] = C[0, 0];
            icrs[4] = C[1, 0];
            icrs[5] = C[2, 0];

            return icrs.ToArray();
        }

        /// <summary>
        /// Convert innertial frame from ICRS to dynamical.
        /// </summary>
        /// <param name="icrs">Equatorial Heliocentric Coordinates - ICRS Frame J2000</param>
        /// <returns>Ecliptic Heliocentric Coordinates - Dynamical Frame J2000</returns>
        public static double[] ICRStoDynamical(double[] icrs)
        {
            double[] dynamical = new double[6];

            double dgrad = Math.PI / 180.0d;
            double sdrad = Math.PI / 180.0d / 3600.0d;
            double eps = (23.0d + 26.0d / 60.0d + 21.411360d / 3600.0d) * dgrad;
            double phi = -0.051880d * sdrad;

            double Seps, Ceps, Sphi, Cphi;
            Seps = Math.Sin(eps);
            Ceps = Math.Cos(eps);
            Sphi = Math.Sin(phi);
            Cphi = Math.Cos(phi);

#if NET7_0

            #region vector matrix mul

            if (Vector256.IsHardwareAccelerated)
            {
                Vector256<double> m1 = Vector256.Create(Cphi, Sphi, 0, 0);
                Vector256<double> m2 = Vector256.Create(-Sphi * Ceps, Cphi * Ceps, Seps, 0);
                Vector256<double> m3 = Vector256.Create(Sphi * Seps, -Cphi * Seps, Ceps, 0);
                Vector256<double> vv = Vector256.Create(icrs[0], icrs[1], icrs[2], 0);
                Vector256<double> vdv = Vector256.Create(icrs[3], icrs[4], icrs[5], 0);

                dynamical[0] = Vector256.Sum(vv * m1);
                dynamical[1] = Vector256.Sum(vv * m2);
                dynamical[2] = Vector256.Sum(vv * m3);

                dynamical[3] = Vector256.Sum(vdv * m1);
                dynamical[4] = Vector256.Sum(vdv * m2);
                dynamical[5] = Vector256.Sum(vdv * m3);
                return dynamical.ToArray();
            }

            #endregion vector matrix mul

#endif

            //Reverse Matrix
            double[,] R_1 = new double[,] {{ Cphi,       Sphi,      0    },
                                           {-Sphi*Ceps,  Cphi*Ceps, Seps },
                                           { Sphi*Seps, -Cphi*Seps, Ceps }};
            // Vector for cardinal coordnate element
            double[,] A = new double[,] { {icrs[0]},
                                          {icrs[1]},
                                          {icrs[2]}};

            double[,] C = MultiplyMatrix(R_1, A);

            dynamical[0] = C[0, 0];
            dynamical[1] = C[1, 0];
            dynamical[2] = C[2, 0];

            A = new double[,] { {icrs[3]},
                                {icrs[4]},
                                {icrs[5]}};

            C = MultiplyMatrix(R_1, A);
            dynamical[3] = C[0, 0];
            dynamical[4] = C[1, 0];
            dynamical[5] = C[2, 0];

            return dynamical.ToArray();
        }
    }
}