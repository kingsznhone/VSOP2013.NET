# GetVariable

Calculate a specific variable of a planet.

```csharp
public double GetVariable(VSOPBody body, int iv, VSOPTime time)
```

**Parameters**

`body` [VSOPBody](../enums.md#fields)

Planet enum.



`iv` [ int](https://learn.microsoft.com/en-us/dotnet/api/system.int32?view=net-9.0)

Variable index

0-5 : a l k h q p



`time`[ VSOPTime](../vsoptime-class.md)

Exclusive time class in VSOP2013.



**Return**

`double`

variable result.

\
