# VSOP2013 .NET Remastered

## What's This?

VSOP was developed and is maintained (updated with the latest data) by the scientists at the Bureau des Longitudes in Paris.

VSOP2013,another version of VSOP, computed the positions of the planets directly at any moment, as well as their orbital elements with improved accuracy.

Original VSOP2013 Solution was write by FORTRAN 77 . It's too old to use.

This repo is not just programming language translation, it's  refactoring of VSOP2013.

This thing is totally useless for myself. But I think someone might need this algorithm in the future.

VSOP2013 is much much slower than VSOP87. But I guess it's more accurate than 87?

## Introduction

The VSOP2013 files contain the series of the elliptic elements for the 8 planets Mercury, Venus, Earth-Moon barycenter, Mars, Jupiter, Saturn, Uranus, and Neptune and for the dwarf planet Pluto of the solution VSOP2013. The planetary solution VSOP2013 is fitted to the numerical integration INPOP10a built at IMCCE, Paris Observatory over the time interval +1890...+2000.

The precision is of a few 0.1″ for the telluric planets (1.6″ for Mars) over the time interval −4000...+8000.

G. FRANCOU & J.-L. SIMON (MAY 2013) 
 
Ref: Simon J.-L., Francou G., Fienga A., Manche H., A&A 557, A49 (2013) 

List of the data files: 

VSOP2013p1.dat : Mercury  

VSOP2013p2.dat : Venus  

VSOP2013p3.dat : Earth-Moon Barycenter 

VSOP2013p4.dat : Mars  

VSOP2013p5.dat : Jupiter  

VSOP2013p6.dat : Saturn  

VSOP2013p7.dat : Uranus  

VSOP2013p8.dat : Neptune  

VSOP2013p9.dat : Pluto 
 
The planetary solution VSOP2013 is fitted to the numerical integration INPOP10a built at IMCCE, Paris Observatory (https://ftp.imcce.fr/pub/ephem/planets/vsop2013/).   


## FILES DESCRIPTION 
 
 Each VSOP2013 file corresponds to a planet and contains trigonometric series, functions of time (Periodic series and Poisson series), that represent the 6 elliptic elements of the planet:  

Variable 1 : a = semi-major axis (ua)  

Variable 2 : λ = mean longitude  (radian)  

Variable 3 : k = e cos ω

Variable 4 : h = e sin ω

Variable 5 : q = sin(i/2) cos Ω

Variable 6 : p = sin(i/2) sin Ω 
 
### with: 

e : eccentricity  

ω : perihelion longitude 

i : inclination 

Ω : ascending node longitude 
 
### VSOP2013 series are characterized by 3 parameters: 

- the planet index 1-9 from Mercury to Pluto,  

- the variable index 1-6 for a, λ, k, h, q, p, 

- the time power α.

## Feature

1. Use VSOPResult class to manage calculate results.
2. Use VSOPTime class to manage time. Easy to convert time by calling VSOPTime.UTC VSOPTime.TAI VSOPTime.TDB
3. Extremely optimized. About 6ms per full result on 5900HX.

![Performance Test](https://github.com/kingsznhone/VSOP2013/blob/master/Performance.png)


I found calculation of iphi it's not related with time. So calculations of aa&bb were done during data reading.

I'm not sure should I Dump the preprocessed data to BIN file or not.

### Pros
- Smaller Data size

### Cons
- Slower on load 
- more memory use.

If program using original data file. Loading time is much less than using binary data file. About 1->4s. It's hard to choose which is better.

Finally I decide to use original data file.

## API
 
### public double CalcIV(VSOPBody body,int iv, VSOPTime time)

Calculate a specific variable of  a planet.

#### Parameters

```body``` VSOPBody

Planet enum.

```iv``` Variable index

0-5 : a l k h q p

```time``` VSOPTime

Exclusive time class in VSOP2013.

#### Return

```double``` variable result.

### public VSOPResult CalcPlanet(VSOPBody body, VSOPTime time)

Calculate all variable of a planet.

#### Parameters

```body``` VSOPBody

Planet enum.

```time``` VSOPTime

Exclusive time class in VSOP2013.

#### Return

```VSOPResult``` Full Result with 6 variable in 3 type of  Coordinates.

- Elliptic Elements Dynamical Frame J2000

- Ecliptic Heliocentric Coordinates Dynamical Frame J2000
 
- Equatorial Heliocentric Coordinates ICRS Frame J2000

### public VSOPResult[] CalcAllPlanet(VSOPTime time)

Calculate all variable of a planet.

#### Parameters

```time``` VSOPTime

Exclusive time class in VSOP2013.

#### Return

```VSOPResult[]``` Full result with 6 variable in 3 type of  Coordinates of all 9 planet.

## Change Log

## 2022.06.02

New features.

Performance Optimization.

Upgrade to .NET 6

## 2020.11.14

Upgrade to .NET 5

## 2020.07.06

Initial PR.

## Enviroment Require

.NET6 Runtime