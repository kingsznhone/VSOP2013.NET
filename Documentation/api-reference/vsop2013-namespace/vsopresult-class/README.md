# VSOPResult Class

## Definition

Base class of calculate result.

```csharp
public abstract class VSOPResult
```

## Properties <a href="#methods" id="methods"></a>

`public` [`VSOPBody`](../enums.md#fields) `Body { get; }`

Planet of this result.



`public` [`VSOPTime`](../vsoptime-class.md) `Time { get; }`

Input time of this result.



`public abstract` [`CoordinatesType`](../enums.md#coordinatestype) `CoordinatesType { get; }`

Coordinates type of this result.



`public` [`ReferenceFrame`](../enums.md#referenceframe) `ReferenceFrame { get; set;}`

ReferenceFrame of this result.&#x20;



`public ReadOnlySpan<double> Variables { get;}`

Raw 6-element coordinate array (read-only view).



## Methods

```cs
public abstract VSOPResult_ELL ToELL()
```

Convert a result to Elliptic Coordinates.



```csharp
public abstract VSOPResult_XYZ ToXYZ()
```

Convert a result to Rectangula Coordinates.



```csharp
public abstract VSOPResult_LBR ToLBR()
```

Convert a result to Spherical Coordinates.



```cs
public abstract VSOPResult ChangeFrame(ReferenceFrame targetFrame);
```

Change reference frame of a result.

**Parameters**

`targetFrame`   [ReferenceFrame](../enums.md#referenceframe)&#x20;

target reference frame to convert

