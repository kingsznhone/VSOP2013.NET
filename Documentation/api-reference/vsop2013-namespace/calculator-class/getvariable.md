# GetVariable

Calculate a specific variable of a planet.

```csharp
public double GetVariable(VSOPBody body, VSOPVariable variable, VSOPTime time)
```

**Parameters**

`body` [VSOPBody](../enums.md#fields)

Planet enum.



`variable` [VSOPVariable](../enums.md#fields-2)

Elliptic element to compute: A, L, K, H, Q or P.



`time`[ VSOPTime](../vsoptime-class.md)

Exclusive time class in VSOP2013.



**Return**

`double`

variable result. L is normalized to [0, 2π).

<br>
