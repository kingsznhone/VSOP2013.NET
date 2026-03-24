# VSOPResult\_XYZ Class

## Definition

VSOP2013 calculate result represent in cartesian coordinate system.

```csharp
public class VSOPResult_XYZ : VSOPResult
```

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public VSOPResult_XYZ(VSOPBody body, VSOPTime time, double[] variables, ReferenceFrame frame)
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

`double x {get;}`

Position x (au)



`double y {get;}`

Position y (au)



`double z {get;}`

Position z (au)



`double dx {get;}`

Velocity x (au/day)



`double dy {get;}`

Velocity y (au/day)



`double dz {get;}`

Velocity z (au/day)
