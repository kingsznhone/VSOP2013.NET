# VSOPResult\_ELL Class

## Definition

VSOP2013 calculate result represent in elliptic coordinate system.

```csharp
public class VSOPResult_ELL : VSOPResult
```

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public VSOPResult_ELL(VSOPBody body, VSOPTime time, double[] variables, ReferenceFrame frame)
```

Create a new elliptic result from calculate result.



**Parameters**

`body` [VSOPBody](../enums.md#fields)

Planet



`time` [VSOPTime](../vsoptime-class.md)

Time wrapper for VSOP.



`variables` double\[]

Raw result data from calculator.

&#x20;

`frame`  [ReferenceFrame](../enums.md#referenceframe)

Reference frame of this result.

***

## Properties <a href="#properties" id="properties"></a>

`public double a {get;}`

Semi-major axis (au)



`public double l {get;}`

Mean longitude (rd)



`public double k {get;}`

e\*cos(pi) (rd)



`public double h {get;}`

e\*sin(pi) (rd)



`public double q {get;}`&#x20;

sin(i/2)\*cos(omega) (rd)



`public double p {get;}`

sin(i/2)\*sin(omega) (rd)

