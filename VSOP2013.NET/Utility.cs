using System.Numerics;
using System.Runtime.Intrinsics;

namespace VSOP2013
{
    public static class Utility
    {
        public static readonly Dictionary<VSOPBody, double> GM = new()
        {
            { VSOPBody.SUN,     2.9591220836841438269e-04d },
            { VSOPBody.MERCURY, 4.9125474514508118699e-11d },
            { VSOPBody.VENUS,   7.2434524861627027000e-10d },
            { VSOPBody.EMB,     8.9970116036316091182e-10d },
            { VSOPBody.MARS,    9.5495351057792580598e-11d },
            { VSOPBody.JUPITER, 2.8253458420837780000e-07d },
            { VSOPBody.SATURN,  8.4597151856806587398e-08d },
            { VSOPBody.URANUS,  1.2920249167819693900e-08d },
            { VSOPBody.NEPTUNE, 1.5243589007842762800e-08d },
            { VSOPBody.PLUTO,   2.1886997654259696800e-12d },
        };

        public static double[,] MultiplyMatrix(double[,] A, double[,] B)
        {
            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);

            if (cA != rB)
            {
                throw new ArgumentException("Matrixes can't be multiplied!!");
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
        /// Convert cartesian coordinate to spherical coordinate
        /// </summary>
        /// <param name="xyz">x,y,z,dx,dy,dz</param>
        /// <returns>l,b,r,dl,db,dr</returns>
        public static double[] XYZtoLBR(double[] xyz)
        {
            double[] lbr = new double[6];

            double x = xyz[0];
            double y = xyz[1];
            double z = xyz[2];
            double dx = xyz[3];
            double dy = xyz[4];
            double dz = xyz[5];
            double l, b, r, dl, db, dr;

            //r = sqrt(x^2+y^2+z^2)
            r = Math.Sqrt(x * x + y * y + z * z);

            //L = atan2(y, x)
            l = Math.Atan2(y, x);

            //θ = arccos(z/r)
            //b = pi/2 - θ.
            //Sin(θ) = Cos(b), Cos(θ) = Sin(b)
            b = Math.Asin(z / r);

            //modulo l into [0,Tau)
            l -= Math.Floor(l / Math.Tau) * Math.Tau;
            if (l < 0)
                l += Math.Tau;

            #region vector matrix mul

            if (Vector256.IsHardwareAccelerated)
            {
                double rho2 = x * x + y * y;
                double rho = Math.Sqrt(rho2);
                Vector256<double> v1 = Vector256.Create(x / r, y / r, z / r, 0);
                Vector256<double> v2 = Vector256.Create(x * z / (r * r * rho), y * z / (r * r * rho), -rho2 / (r * r * rho), 0);
                Vector256<double> v3 = Vector256.Create(-y / rho2, x / rho2, 0, 0);

                Vector256<double> vv = Vector256.Create(dx, dy, dz, 0);

                lbr[0] = l;
                lbr[1] = b;
                lbr[2] = r;
                lbr[3] = Vector256.Sum(v3 * vv);
                lbr[4] = -Vector256.Sum(v2 * vv);
                lbr[5] = Vector256.Sum(v1 * vv);
                return lbr;
            }

            #endregion vector matrix mul

            // Inverse Jacobian matrix  From  cartesian to spherical
            //https://en.wikipedia.org/wiki/Spherical_coordinate_system#Integration_and_differentiation_in_spherical_coordinates
            double[,] Inverse_J = {
                           {x/r                          , y/r                         , z/r                                 },
                           {x*z/(r*r*Math.Sqrt(x*x+y*y)) , y*z/(r*r*Math.Sqrt(x*x+y*y)),-(x*x+y*y)/(r*r*Math.Sqrt(x*x+y*y))  },
                           {-y/(x*x+y*y)                 , x/(x*x+y*y)                 , 0                                   }};
            double[,] Velocity = { { dx }, { dy }, { dz } };
            var C = MultiplyMatrix(Inverse_J, Velocity);
            dl = C[2, 0];
            db = C[1, 0];
            dr = C[0, 0];

            lbr[0] = l;
            lbr[1] = b;
            lbr[2] = r;
            lbr[3] = dl;
            lbr[4] = -db;
            lbr[5] = dr;
            return lbr;
        }

        /// <summary>
        /// convert spherical coordinate to cartesian coordinate
        /// </summary>
        /// <param name="lbr">l,b,r,dl,db,dr</param>
        /// <returns>x,y,z,dx,dy,dz</returns>
        public static double[] LBRtoXYZ(double[] lbr)
        {
            double[] xyz = new double[6];
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
                xyz[3] = Vector256.Sum(m1 * vv);
                xyz[4] = Vector256.Sum(m2 * vv);
                xyz[5] = -Vector256.Sum(m3 * vv);
                return xyz;
            }

            #endregion vector matrix mul

            // Jacobian matrix From spherical to cartesian
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
            return xyz;
        }

        /// <summary>
        /// Convert Elliptic coordinate to cartesian coordinate.
        /// This is kind of magic that I will never understand.
        /// Directly translate from VSOP2013.f.
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="ell">Elliptic Elements: a,l,k,h,q,p </param>
        /// <returns>cartesian Heliocentric Coordinates</returns>
        public static double[] ELLtoXYZ(VSOPBody body, double[] ell)
        {
            double[] xyz = new double[6];
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
            rgm = Math.Sqrt(GM[body] + GM[VSOPBody.SUN]);

            //Computation
            xfi = Math.Sqrt(1.0d - (k * k) - (h * h));
            xki = Math.Sqrt(1.0d - (q * q) - (p * p));
            u = 1.0d / (1.0d + xfi);
            z = new Complex(k, h);
            ex = z.Magnitude;
            ex2 = ex * ex;
            ex3 = ex * ex * ex;
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
                if (Math.Abs(dl) < 1e-15) break;
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
            xn = rgm / (a * Math.Sqrt(a));

            xyz[3] = xn * ((2.0d * p * p - 1.0d) * xms + 2.0d * p * q * xmc);
            xyz[4] = xn * ((1.0d - 2.0d * q * q) * xmc - 2.0d * p * q * xms);
            xyz[5] = xn * 2.0d * xki * (p * xms + q * xmc);

            return xyz;
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
        /// Convert cartesian coordinate to Elliptic coordinate.
        /// Inverse of ELLtoXYZ.
        /// Powered By Claude Opus 4.6
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="xyz">cartesian Heliocentric Coordinates: x,y,z,dx,dy,dz</param>
        /// <returns>Elliptic Elements: a,l,k,h,q,p</returns>
        public static double[] XYZtoELL(VSOPBody body, double[] xyz)
        {
            double[] ell = new double[6];

            double x = xyz[0];
            double y = xyz[1];
            double z = xyz[2];
            double dx = xyz[3];
            double dy = xyz[4];
            double dz = xyz[5];

            double mu = GM[body] + GM[VSOPBody.SUN];

            // Distance and velocity squared
            double r = Math.Sqrt(x * x + y * y + z * z);
            double v2 = dx * dx + dy * dy + dz * dz;

            // Semi-major axis (vis-viva equation)
            double a = mu * r / (2.0d * mu - r * v2);

            // Angular momentum vector: h_vec = r_vec × v_vec
            double hvx = y * dz - z * dy;
            double hvy = z * dx - x * dz;
            double hvz = x * dy - y * dx;
            double hMag = Math.Sqrt(hvx * hvx + hvy * hvy + hvz * hvz);

            // Inclination components q, p using sin(i/2) convention
            // q = sin(i/2)cos(Ω), p = sin(i/2)sin(Ω)
            // Derived from: q = -h_y / (2|h|cos(i/2)), p = h_x / (2|h|cos(i/2))
            // where 2|h|cos(i/2) = sqrt(2|h|(|h|+h_z))
            double qpDenom = Math.Sqrt(2.0d * hMag * (hMag + hvz));
            double q = -hvy / qpDenom;
            double p = hvx / qpDenom;

            double xki = Math.Sqrt(1.0d - q * q - p * p);

            // Equinoctial frame unit vectors (from ELLtoXYZ rotation)
            // ê₁ and ê₂ span the orbital plane
            double e1x = 1.0d - 2.0d * p * p;
            double e1y = 2.0d * p * q;
            double e1z = -2.0d * p * xki;

            double e2x = 2.0d * p * q;
            double e2y = 1.0d - 2.0d * q * q;
            double e2z = 2.0d * q * xki;

            // Eccentricity vector: e_vec = ((v²-μ/r)·r_vec - (r·v)·v_vec) / μ
            double rdotv = x * dx + y * dy + z * dz;
            double coeff = v2 - mu / r;
            double ex = (coeff * x - rdotv * dx) / mu;
            double ey = (coeff * y - rdotv * dy) / mu;
            double ez = (coeff * z - rdotv * dz) / mu;

            // Eccentricity components by projection onto equinoctial frame
            double k = ex * e1x + ey * e1y + ez * e1z;
            double h = ex * e2x + ey * e2y + ez * e2z;

            double ecc = Math.Sqrt(k * k + h * h);
            double xfi = Math.Sqrt(1.0d - k * k - h * h);

            // True longitude F from position projection onto orbital plane
            double f = (x * e1x + y * e1y + z * e1z) / r;
            double g = (x * e2x + y * e2y + z * e2z) / r;
            double F = Math.Atan2(g, f);

            // Convert true longitude to eccentric longitude then mean longitude
            // ω̄ = atan2(h, k), ν = F - ω̄ (true anomaly)
            double omegaBar = Math.Atan2(h, k);
            double nu = F - omegaBar;

            // Eccentric anomaly E' from true anomaly ν (exact formula)
            double cosNu = Math.Cos(nu);
            double sinNu = Math.Sin(nu);
            double denomE = 1.0d + ecc * cosNu;
            double cosEp = (ecc + cosNu) / denomE;
            double sinEp = xfi * sinNu / denomE;
            double Ep = Math.Atan2(sinEp, cosEp);

            // Eccentric longitude E = E' + ω̄
            double E = Ep + omegaBar;

            // Mean longitude L from equinoctial Kepler equation: L = E - k·sin(E) + h·cos(E)
            double L = E - k * Math.Sin(E) + h * Math.Cos(E);

            ell[0] = a;
            ell[1] = L;
            ell[2] = k;
            ell[3] = h;
            ell[4] = q;
            ell[5] = p;

            return ell;
        }

        /// <summary>
        /// Convert spherical coordinate to Elliptic coordinate.
        /// </summary>
        /// <param name="body">planet</param>
        /// <param name="lbr">Spherical Heliocentric Coordinates: l,b,r,dl,db,dr</param>
        /// <returns>Elliptic Elements: a,l,k,h,q,p</returns>
        public static double[] LBRtoELL(VSOPBody body, double[] lbr)
        {
            double[] xyz = LBRtoXYZ(lbr);
            return XYZtoELL(body, xyz);
        }

        /// <summary>
        /// Convert inertial frame from dynamical to ICRS.
        /// </summary>
        /// <param name="dynamical">Ecliptic Heliocentric Coordinates - Dynamical Frame J2000</param>
        /// <returns>Equatorial Heliocentric Coordinates - ICRS Frame J2000</returns>
        public static double[] DynamicaltoICRS(double[] dynamical)
        {
            double[] icrs = new double[6];

            double dgrad = Math.PI / 180.0d;
            double sdrad = Math.PI / 180.0d / 3600.0d;
            double eps = (23.0d + 26.0d / 60.0d + 21.411360d / 3600.0d) * dgrad;
            double phi = -0.051880d * sdrad;

            double Seps, Ceps, Sphi, Cphi;
            Seps = Math.Sin(eps);
            Ceps = Math.Cos(eps);
            Sphi = Math.Sin(phi);
            Cphi = Math.Cos(phi);

            if (Vector256.IsHardwareAccelerated)
            {
                Vector256<double> r1 = Vector256.Create(Cphi, -Sphi * Ceps, Sphi * Seps, 0);
                Vector256<double> r2 = Vector256.Create(Sphi, Cphi * Ceps, -Cphi * Seps, 0);
                Vector256<double> r3 = Vector256.Create(0, Seps, Ceps, 0);

                Vector256<double> vv = Vector256.Create(dynamical[0], dynamical[1], dynamical[2], 0);
                Vector256<double> vdv = Vector256.Create(dynamical[3], dynamical[4], dynamical[5], 0);

                icrs[0] = Vector256.Sum(vv * r1);
                icrs[1] = Vector256.Sum(vv * r2);
                icrs[2] = Vector256.Sum(vv * r3);

                icrs[3] = Vector256.Sum(vdv * r1);
                icrs[4] = Vector256.Sum(vdv * r2);
                icrs[5] = Vector256.Sum(vdv * r3);
                return icrs;
            }

            //Rotation Matrix
            double[,] R = new double[,] { {Cphi, -Sphi*Ceps,  Sphi*Seps },
                                          {Sphi,  Cphi*Ceps, -Cphi*Seps },
                                          {0,     Seps,       Ceps      }};

            // Vector for cartesian coordinate element
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

            return icrs;
        }

        /// <summary>
        /// Convert inertial frame from ICRS to dynamical.
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
                return dynamical;
            }

            #endregion vector matrix mul

            //Reverse Matrix
            double[,] R_1 = new double[,] {{ Cphi,       Sphi,      0    },
                                           {-Sphi*Ceps,  Cphi*Ceps, Seps },
                                           { Sphi*Seps, -Cphi*Seps, Ceps }};
            // Vector for cartesian coordinate element
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

            return dynamical;
        }
    }
}
