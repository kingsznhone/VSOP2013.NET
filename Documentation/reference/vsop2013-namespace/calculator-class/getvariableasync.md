# GetVariableAsync

Calculate a specific variable of a planet.

```csharp
public async Task<double> GetVariableAsync(VSOPBody body, int iv, VSOPTime time)
```

**Parameters**

`body` [VSOPBody](../enums.md#fields)

Planet enum.



`iv`  int

Variable index

0-5 : a l k h q p



`time`[ VSOPTime](../vsoptime-class.md)

Exclusive time class in VSOP2013.



**Return**

`Task<double>`

variable result.

\
