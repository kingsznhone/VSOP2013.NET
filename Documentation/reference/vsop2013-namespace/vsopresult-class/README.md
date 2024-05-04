# VSOPResult Class

## Definition

Base class of calculate result.

```csharp
public abstract class VSOPResult
```

## Properties <a href="#methods" id="methods"></a>

`public abstract` [`VSOPBody`](../enums.md#fields) `Body { get; }`

Planet of this result.



`public abstract` [`CoordinatesReference`](../enums.md#coordinatesreference) `CoordinatesReference{ get; }`

Coordinates Reference of this result.



`public abstract` [`CoordinatesType`](../enums.md#coordinatestype) `CoordinatesType { get; }`

Coordinates type of this result.



`public abstract` [`ReferenceFrame`](../enums.md#referenceframe) `ReferenceFrame { get; set;}`

ReferenceFrame of this result. Set to `ReferenceFrame.ICRSJ2000` or `ReferenceFrame.DynamicalJ2000` will automatically change coordinate field.



`public abstract` [`VSOPTime`](../vsoptime-class.md) `Time { get; }`

Input time of this result.



`public abstract double[] Variables_ELL { get;}`

Raw data of this result in elliptic coordinate.

