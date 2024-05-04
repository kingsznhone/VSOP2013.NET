# Terms of Series

The terms of series are given under the form: &#x20;

$$
T^α (S*sin(Φ)+C*cos(Φ))
$$

T is the time (TDB) from J2000 (JD2451545.0) expressed in Thousand of Julian Years (tcy = 365250 days)

**α** is the time power of the series (0 <= **α** <= 20).

S, C are the coefficients for the variable a (au), the variable **λl** (radian) and the variables, k, h, q, p (without unit).

**Φ** is equal to the sum of the products a(i).**λl**(i) with i=1,17.

a(i) are integers, numerical coefficients of the quantities **λl**(i).



λl(1,13) : linear part of the mean longitudes of the planets (radian).

λl(14) : argument **µ** derived from TOP2013 and used for Pluto (radian).

λl(15,17) : linear part of Delaunay lunar arguments D, F, **ℓ** (radian).



λl(1) = 4.402608631669 + 26087.90314068555 \* T Mercury

λl(2) = 3.176134461576 + 10213.28554743445 \* T Venus

λl(3) = 1.753470369433 + 6283.075850353215 \* T Earth-Moon

λl(4) = 6.203500014141 + 3340.612434145457 \* T Mars

λl(5) = 4.091360003050 + 1731.170452721855 \* T Vesta

λl(6) = 1.713740719173 + 1704.450855027201 \* T Iris

λl(7) = 5.598641292287 + 1428.948917844273 \* T Bamberga

λl(8) = 2.805136360408 + 1364.756513629990 \* T Ceres

λl(9) = 2.326989734620 + 1361.923207632842 \* T Pallas

λl(10) = 0.599546107035 + 529.6909615623250 \* T Jupiter

λl(11) = 0.874018510107 + 213.2990861084880 \* T Saturn

λl(12) = 5.481225395663 + 74.78165903077800 \* T Uranus

λl(13) = 5.311897933164 + 38.13297222612500 \* T Neptune

λl(14) =                             0.3595362285049309 \* T **µ** Pluto

λl(15) = 5.198466400630 + 77713.7714481804 \* T D Moon

λl(16) = 1.627905136020 + 84334.6615717837 \* T F Moon

λl(17) = 2.355555638750 + 83286.9142477147 \* T **ℓ** Moon



VSOP2013 files contain the numerical values of **α**, S, C and a(i) (i=1,17).

