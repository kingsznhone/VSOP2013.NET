# VSOPResult\_LBR Class

## Definition

VSOP2013 calculate result represent in spherical coordinate system.

```csharp
public class VSOPResult_LBR : VSOPResult
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

`double l {get;}`

longitude (rd)



`double b {get;}`

latitude (rd)



`double r {get;}`

radius (au)



`double dl {get;}`

longitude velocity (rd/day)



`double db {get;}`

latitude velocity (rd/day)



`double dr {get;}`

radius velocity (au/day)
