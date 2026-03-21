# Files Description

Each VSOP2013 file corresponds to a planet and contains trigonometric series, functions of time (Periodic series and Poisson series), that represent the 6 elliptic elements of the planet:

| Variable 1 | a = semi-major axis (ua)    |
| ---------- | --------------------------- |
| Variable 2 | λ = mean longitude (radian) |
| Variable 3 | k = e cos ω                 |
| Variable 4 | h = e sin ω                 |
| Variable 5 | q = sin(i/2) cos Ω          |
| Variable 6 | p = sin(i/2) sin Ω          |

with:

* e : eccentricity
* **ω** : perihelion longitude
* i : inclination
* **Ω** : ascending node longitude

VSOP2013 series are characterized by 3 parameters:

* the planet index 1-9 from Mercury to Pluto,
* the variable index 1-6 for a, **λ**, k, h, q, p,
* the time power **α**.
