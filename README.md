# VSOP2013 .NET 

[![NuGet package](https://img.shields.io/nuget/v/VSOP2013.NET.svg?logo=NuGet)](https://www.nuget.org/packages/VSOP2013.NET/)
[![NuGet package](https://img.shields.io/nuget/dt/VSOP2013.NET?logo=NuGet)](https://www.nuget.org/packages/VSOP2013.NET/)

## What's this?

VSOP was developed and is maintained (updated with the latest data) by the scientists at the Bureau des Longitudes in Paris.

VSOP2013,another version of VSOP, computed the positions of the planets directly at any moment, as well as their orbital elements with improved accuracy.

Original VSOP2013 Solution was write by FORTRAN 77 . It's too old to use.

This repo is not just programming language translation, it's  refactoring of VSOP2013.

This thing is totally useless for myself. But I think someone might need this algorithm in the future.

VSOP2013 is much much slower than VSOP87. But  it's more accurate than 87?

I use multi-thread and precalculation technique to accelerate iteration speed.

I promise it will be much faster than origin algorithm.

This is the best VSOP2013 library ever.

![Demo](https://github.com/kingsznhone/VSOP2013.NET/blob/main/Demo.png)

## Features

1. Use VSOPResult class to manage calculate results.
2. Use VSOPTime class to manage time. 
<br>Easy to convert time by calling ```VSOPTime.UTC```, ```VSOPTime.TAI```, ```VSOPTime.TDB```
3. Veryhigh performance per solution
<br>![Performance Test](https://github.com/kingsznhone/VSOP2013.NET/blob/main/PerformanceTest.png)
4. Useful Utility class. Convert Elliptic coordinates to cartesian  or spherical 
5. Async Api.
6. precalculation on <b>φ</b> in terms, which gives 20%+ speed up of calculation.
7. Use [MessagePack](https://github.com/neuecc/MessagePack-CSharp#lz4-compression"MessagePack for C#") for binary serialize.
<br>Initialization time becomes less than 10% of previous.
8. Brotli compression on source data. ~300Mb -> ~50MB.

<br>

## Introduction

The VSOP2013 files contain the series of the elliptic elements for the 8 planets Mercury, Venus, Earth-Moon barycenter, Mars, Jupiter, Saturn, Uranus, and Neptune and for the dwarf planet Pluto of the solution VSOP2013. The planetary solution VSOP2013 is fitted to the numerical integration INPOP10a built at IMCCE, Paris Observatory over the time interval +1890...+2000.

The precision is of a few 0.1″ for the telluric planets (1.6″ for Mars) over the time interval −4000...+8000.

G. FRANCOU & J.-L. SIMON (MAY 2013) 
 
Ref: Simon J.-L., Francou G., Fienga A., Manche H., A&A 557, A49 (2013) 
 
The planetary solution VSOP2013 is fitted to the numerical integration INPOP10a built at IMCCE, Paris Observatory (https://ftp.imcce.fr/pub/ephem/planets/vsop2013/).   


## How to use

* NuGet Package Manager
    ```
    PM> NuGet\Install-Package VSOP2013.NET -Version 1.1.6
    ```

```
using VSOP2013;

Calculator vsop = new Calculator();

// Create VSOPTime using UTC .
DateTime Tinput = DateTime.Now;
VSOPTime vTime = new VSOPTime(Tinput.ToUniversalTime(),TimeFrame.UTC);

// Calculate EMB's present position
VSOPResult_ELL ell = vsop.GetPlanetPosition(VSOPBody.EMB, vTime);

// Convert to diffirent coordinate system.
VSOPResult_XYZ xyz=ell.ToXYZ();
VSOPResult_LBR lbr=ell.ToLBR();

// OR
xyz = (VSOPResult_XYZ)ell;
lbr = (VSOPResult_LBR)ell;

// Print result
Console.WriteLine($"Body: {Enum.GetName(ell.Body)}");
Console.WriteLine($"Coordinates Type: {Enum.GetName(ell.CoordinatesType)}");
Console.WriteLine($"Coordinates Reference: {Enum.GetName(ell.CoordinatesReference)}");
Console.WriteLine($"Reference Frame: {Enum.GetName(ell.ReferenceFrame)}");
Console.WriteLine($"Time UTC: {ell.Time.UTC.ToString("o")}");
Console.WriteLine($"Time TDB: {ell.Time.TDB.ToString("o")}");
Console.WriteLine("---------------------------------------------------------------");
Console.WriteLine(String.Format("{0,-33}{1,30}", "semi-major axis (au)", ell.a));
Console.WriteLine(String.Format("{0,-33}{1,30}", "mean longitude (rad)", ell.l));
Console.WriteLine(String.Format("{0,-33}{1,30}", "k = e*cos(pi) (rad)", ell.k));
Console.WriteLine(String.Format("{0,-33}{1,30}", "h = e*sin(pi) (rad)", ell.h));
Console.WriteLine(String.Format("{0,-33}{1,30}", "q = sin(i/2)*cos(omega) (rad)", ell.q));
Console.WriteLine(String.Format("{0,-33}{1,30}", "p = sin(i/2)*sin(omega) (rad)", ell.p));
Console.WriteLine("===============================================================");
```

## Change Log

### 2024.01.14 v1.1.8

Fix critical error in ELL to XYZ convertion. 

### 2023.12.13 v1.1.7

Bug fix.

Add explicit cast of VSOPResult

### 2023.9.3 v1.1.6

SIMD Accel Utility. 

Bug Fix

### 2023.7.7 v1.1.5

Bug Fix.

API change with some feature in VSOP87

### 2023.7.05 v1.1.2

Many  improvements.

Some of them are from VSOP87.NET

### 2023.03.26 v1.1.1

Initialize performance upgrade.

Add result classes.

Use MessagePack to compress original data.

### 2022.06.02 v1.0.0

New features.

Performance Optimization.

Upgrade to .NET 6

### 2020.11.14 v0.9b

Upgrade to .NET 5

### 2020.07.06 v0.1b

Initial PR.

<br>

## Enviroment Require

.NET6 Runtime

.NET7 Runtime

.NET8 Runtime (Planned) 

Windows 10 64bit or Higher

## Reference
 
 [MessagePack](https://github.com/neuecc/MessagePack-CSharp#lz4-compression"MessagePack for C#")

<br>

# API
 
## Class Calculator

Provide methods to calculate planet position.

#### Methods

### ```public double GetVariable(VSOPBody body,int iv, VSOPTime time)```

Calculate a specific variable of  a planet.

<br>

#### Parameters

```body``` VSOPBody

Planet enum.

<br>

```iv``` Variable index

0-5 : a l k h q p

<br>

```time``` VSOPTime

Exclusive time class in VSOP2013.

<br>

#### Return

```double``` variable result.

<br>

### ```public Task<double> GetVariableAsync(VSOPBody body,int iv, VSOPTime time)```

Calculate a specific variable of  a planet.

<br>

#### Parameters

```body``` VSOPBody

Planet enum.

<br>

```iv``` Variable index

0-5 : a l k h q p

<br>

```time``` VSOPTime

Exclusive time class in VSOP2013.

<br>

#### Return

```Task<double>```

variable result.

<br>

### ```public VSOPResult_ELL GetPlanetPosition(VSOPBody body, VSOPTime time)```

Calculate all variable of a planet.

<br>

#### Parameters

```body``` VSOPBody

Planet enum.

<br>

```time``` VSOPTime

Exclusive time class in VSOP2013.

<br>

#### Return

```VSOPResult_ELL``` 

Full Result with 6 variable of Elliptic  Coordinates.

Can be explicit cast to ```VSOPResult_XYZ``` and ```VSOPResult_LBR```

<br>

### ```public Task<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)```

Calculate all variable of a planet.

<br>

#### Parameters

```body``` VSOPBody

Planet enum.

<br>

```time``` VSOPTime

Exclusive time class in VSOP2013.

<br>

#### Return

```Task<VSOPResult_ELL>``` 

Full Result with 6 variable of Elliptic  Coordinates.

Can be explicit cast to ```VSOPResult_XYZ``` and ```VSOPResult_LBR```

<br>

## Static Class Utility

This Class Provide some useful function.

<br>

#### Methods

### ```static double[,] MultiplyMatrix(double[,] A, double[,] B)```

A handy matrix multiply function

#### Parameters

```A``` double[,]

Matrix A

<br>

```B``` double[,]

Matrix B

<br>

#### Return

```double[,]```

Matrix C = AB.

<br>

### ```static double[] XYZtoLBR(double[] xyz)```

#### Parameters

```xyz``` double[]

Array of cartesian coordinate elements

<br>

#### Return

```double[]```

Array of spherical coordinate elements

<br>

### ```static double[] LBRtoXYZ(double[] lbr)```

#### Parameters

```lbr``` double[]

Array of spherical coordinate elements

<br>

#### Return

```double[]```

Array of cartesian coordinate elements

<br>

### ```static double[] ELLtoXYZ(double[] ell)```

This is a magic function I directly copy from VSOP2013.

It's way beyond my math level.

So I can't find how to inverse XYZ elements to ELL elements.

Need help.

#### Parameters

```ell``` double[]

Array of elliptic coordinate elements

<br>

#### Return

```double[]```

Array of cartesian coordinate elements

<br>

### ```static double[] ELLtoLBR(double[] ell)```

#### Parameters

```ell``` double[]

Array of elliptic coordinate elements

<br>

#### Return

```double[]```

Array of spherical coordinate elements

<br>

### ```static double[] DynamicaltoICRS(double[] xyz)```

#### Parameters

```xyz``` double[]

Array of cartesian coordinate elements that inertial frame of dynamical equinox and ecliptic.

<br>

#### Return

```double[]```

Array of cartesian coordinate elements that inertial frame of ICRS equinox and ecliptic.

<br>

### ```static double[] ICRStoDynamical(double[] xyz)```

#### Parameters

```xyz``` double[]

Array of cartesian coordinate elements that inertial frame of ICRS equinox and ecliptic.

<br>

#### Return

```double[]```

Array of cartesian coordinate elements that inertial frame of dynamical equinox and ecliptic.

<br>

## Class VSOPResult_XYZ : VSOPResult

#### Constructor

### ```VSOPResult_XYZ(VSOPResult_LBR result)```

Create a new cartesian result from spherical result. 

<br>

#### Arguments

```result``` VSOPResult_LBR 

<br>

### ```VSOPResult_XYZ(VSOPResult_ELL result)```

Create a new Cartisian result from ellipitic result. 

<br>

#### Arguments

```result``` VSOPResult_ELL

<br>

#### Properties

```VSOPBody Body { get; }```

Planet of this result.

<br>

```CoordinatesType CoordinatesType { get; }```

Coordinates type of this result.

<br>

```CoordinatesReference CoordinatesReference{ get; }```

Coordinates Reference of this result.

<br>

```ReferenceFrame ReferenceFrame { get; set;}```

ReferenceFrame of this result. Set to ```ReferenceFrame.ICRSJ2000``` or ```ReferenceFrame.DynamicalJ2000``` will automatically change coordinate field.

<br>

```VSOPTime Time  { get; }```

Input time of this result.

<br>

```double[] Variables_ELL { get;}```

Raw data of this result in elliptic coordinate.

<br>

```double[] Variables_XYZ { get;}```

Cartesian coordinate of this result.

<br>

```double x {get;}```

Position x (au)

```double y {get;}```

Position y (au)

```double z {get;}```

Position z (au)

```double dx {get;}```

Velocity x (au/day)

```double dy {get;}```

Velocity y (au/day)

```double dz {get;}```

Velocity z (au/day)

<br>

#### Methods

### ```VSOPResult_LBR ToLBR()```

Convert this result to Spherical coordinate.

<br>


## Class VSOPResult_ELL : VSOPResult

#### Constructor

### ```public VSOPResult_ELL(VSOPBody body, VSOPTime time, double[] ell)```

Create a new elliptic result from calculate result. 

<br>

### ```public VSOPResult_ELL(VSOPResult_XYZ result)```

Create a new elliptic result from cartesian result. 

<br>


### ```public VSOPResult_ELL(VSOPResult_LBR result)```

Create a new elliptic result from spherical result. 

<br>


#### Arguments

```body``` VSOPBody

Planet 

<br>

```time``` VSOPTime

Time wrapper for VSOP

<br>

```ell``` double[]

Raw result data from calculator.

<br>

#### Properties

```VSOPBody Body { get; }```

Planet of this result.

<br>

```CoordinatesType CoordinatesType { get; }```

Coordinates type of this result.

<br>

```CoordinatesReference CoordinatesReference { get; }```

Coordinates Reference of this result.

```ReferenceFrame ReferenceFrame { get; }```

Elliptic Coordinate can't change Reference Frame.

<br>

```VSOPTime Time  { get; }```

Input time of this result.

<br>

```double[] Variables_ELL { get;}```

Raw data of this result in elliptic coordinate.

<br>


```double a {get;}```

Semi-major axis (au)

```double l {get;}```

Mean longitude (rd)

```double k {get;}```

e*cos(pi) (rd)

```double h {get;}```

e*sin(pi) (rd)

```double q {get;}```
sin(i/2)*cos(omega) (rd)

```double p {get;}```

sin(i/2)*sin(omega) (rd)

<br>

#### Methods

### ```VSOPResult_XYZ ToXYZ()```

Convert this result to cartisian coordinate.

<br>

### ```VSOPResult_LBR ToLBR()```

Convert this result to spherical coordinate.

<br>

## Class VSOPResult_LBR : VSOPResult

#### Constructor

### ```VSOPResult_LBR(VSOPResult_XYZ result)```

Create a new spherical result from cartesian result. 

<br>

#### Arguments

```result``` VSOPResult_XYZ 

<br>

### ```VSOPResult_LBR(VSOPResult_ELL result)```

Create a new spherical result from ellipitic result. 

<br>

#### Arguments

```result``` VSOPResult_ELL

<br>

#### Properties

```VSOPBody Body { get; }```

Planet of this result.

<br>

```CoordinatesType CoordinatesType { get; }```

Coordinates type of this result.

<br>

```CoordinatesReference CoordinatesReference { get; }```

Coordinates Reference of this result.

```ReferenceFrame ReferenceFrame { get; set;}```

ReferenceFrame of this result. Set to ```ReferenceFrame.ICRSJ2000``` or ```ReferenceFrame.DynamicalJ2000``` will automatically change coordinate field.

<br>

```VSOPTime Time  { get; }```

Input time of this result.

<br>

```double[] Variables_ELL { get;}```

Raw data of this result in elliptic coordinate.

<br>

```double[] Variables_LBR { get;}```

Spherical coordinate of this result.

<br>

```double l {get;}```

longitude (rd)

```double b {get;}```

latitude (rd)

```double r {get;}```

radius (au)

```double dl {get;}```

longitude velocity (rd/day)

```double db {get;}```

latitude velocity (rd/day)

```double dr {get;}```

radius velocity (au/day)

<br>

#### Methods

### ```VSOPResult_XYZ ToXYZ()```

Convert this result to cartisian coordinate.

<br>

## Class VSOPTime

This class provide time convert and management for VSOP87.

<br>

#### Constructor

### ```VSOPTime(DateTime UTC)```

Use UTC Time to initialize VSOPTime.

<br>

#### Properties

```DateTime UTC```

UTC Time frame.

<br>

```DateTime TAI```

International Atomic Time

<br>

```DateTime TT```

Terrestrial Time (aka. TDT)

<br>

```DateTime TDB```

Barycentric Dynamical Time 

VSOP87 use this time frame.

```double J2000```

Get J2000 from TDB.

<br>

#### Methods

### ```static DateTime ChangeFrame(DateTime dt, TimeFrame SourceFrame, TimeFrame TargetFrame)```

#### Parameters

```dt``` DateTime

A Datetime of any frame.

<br>

```SourceFrame``` TimeFrame

Time frame of ```dt```

<br>

```TargetFrame``` TimeFrame

Target time frame.

<br>

#### Return

```DateTime```

Datetime of target time Frame.

<br>

#### Return

```DateTime```

Datetime of target time Frame.

<br>

### ```static double ToJ2000(DateTime dt)```

#### Parameters

```dt``` DateTime

Datetime to convert

<br>

#### Return

```double```

Julian date.

<br>

### ```static DateTime FromJ2000(double JD)```

#### Parameters

```double```

Julian date to analyze.

<br>

#### Return

```dt``` DateTime

Datetime Class.

<br>




