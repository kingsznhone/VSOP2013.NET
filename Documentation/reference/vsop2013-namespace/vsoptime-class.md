# VSOPTime Class

## Definition

This class provide time convert and management for VSOP2013.

```csharp
public class VSOPTime
```

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public VSOPTime(DateTime dt, TimeFrame sourceframe)
```

Use Datetime to initialize [VSOPTime](vsoptime-class.md). &#x20;



#### **Parameters**

`dt` Datetime

A Datetime struct.



`sourceframe` [TimeFrame](enums.md#fields-4)

TimeFrame of dt. Usually UTC.

***

## **Properties**

`DateTime UTC`

UTC Time frame.



`DateTime TAI`

International Atomic Time



`DateTime TT`

Terrestrial Time (aka. TDT)



`DateTime TDB`

Barycentric Dynamical Time. VSOP2013 use this time frame in calculation.



`double J2000`

Get J2000 from TDB.

***

## Methods <a href="#methods" id="methods"></a>

### ChangeFrame

```csharp
static DateTime ChangeFrame(DateTime dt, TimeFrame SourceFrame, TimeFrame TargetFrame)
```

Convert a Datetime to specific time frame.

#### **Parameters**

`dt` DateTime

A Datetime of any frame.



`SourceFrame` [TimeFrame](enums.md#fields-4)

Time frame of `dt`



`TargetFrame` [TimeFrame](enums.md#fields-4)

Target time frame.



#### **Return**

`DateTime`

Datetime of target time Frame.



***

### ToJ2000

```csharp
static double ToJ2000(DateTime dt)
```

Convert a Datetime to J2000.

#### **Parameters**

`dt` DateTime

Datetime to convert

#### **Return**

`double`

Julian date.



***

### FromJ2000

```csharp
static DateTime FromJ2000(double JD)
```

Convert a J2000 to Datetime.

#### **Parameters**

`double`

Julian date to analyze.

#### **Return**

`dt` DateTime

Datetime Class.

\
