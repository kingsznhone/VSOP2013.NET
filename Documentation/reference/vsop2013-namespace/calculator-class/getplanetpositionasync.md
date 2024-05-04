# GetPlanetPositionAsync

Calculate all 6 variable of a planet.

```csharp
public async Task<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)
```

**Parameters**

`body` [VSOPBody](../enums.md#fields)

Planet enum.



`time` [VSOPTime](../vsoptime-class.md)

Exclusive time class in VSOP2013.



**Return**

`Task<`[`VSOPResult_ELL`](../vsopresult-class/vsopresult\_ell-class.md)`>`

Full Result with 6 variable of Elliptic Coordinates.

Can be explicit cast to [`VSOPResult_XYZ`](../vsopresult-class/vsopresult\_xyz-class.md) and [`VSOPResult_LBR`](../vsopresult-class/vsopresult\_lbr-class.md)

\
