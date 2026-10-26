# Calculator Class

## Definition

Full precision VSOP2013 calculator (double precision). It matches the official `VSOP2013.ctl` reference values to 1e-10.

Full 6 variable or one of them.

```csharp
public partial class Calculator
```

An instance is read-only after construction and safe to share across threads. Create it once and reuse it; construction loads and decompresses the data file.

For fast, low precision visualization use [CalculatorF](../calculatorf-class.md).

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public Calculator();
public Calculator(SimdMode mode, double truncation = 0d);
```

**Parameters**

`mode` [SimdMode](../enums.md#simdmode)

Vector width used to evaluate the series. `SimdMode.Auto` picks the widest hardware accelerated width.

`truncation` double

Truncation level ρ as defined by VSOP2013. Terms with `sqrt(C² + S²) < ρ` are dropped when the data is loaded (au for A, rad for L, unitless for K/H/Q/P). `0` (default) keeps the full series.

**Exceptions**

`PlatformNotSupportedException`: the requested vector width is not hardware accelerated.

`ArgumentOutOfRangeException`: `truncation` is negative or not finite.

## Properties <a href="#properties" id="properties"></a>

`Mode` [SimdMode](../enums.md#simdmode)

Vector width actually used by this instance after hardware detection. It is never `Auto`.

`Truncation` double

Truncation level applied to this instance.

## Methods <a href="#methods" id="methods"></a>

* [GetPlanetPosition(VSOPBody body, VSOPTime time)](getplanetposition.md)
* [GetPlanetPositionAsync(VSOPBody body, VSOPTime time)](getplanetpositionasync.md)
* [GetVariable(VSOPBody body, VSOPVariable variable, VSOPTime time)](getvariable.md)
* [GetVariableAsync(VSOPBody body, VSOPVariable variable, VSOPTime time)](getvariableasync.md)

## Example

```csharp
// Default: full series, widest SIMD width.
Calculator vsop = new Calculator();

// Scalar evaluation, drop terms smaller than 1e-14.
Calculator scalar = new Calculator(SimdMode.Scalar, truncation: 1e-14);
Console.WriteLine(scalar.Mode); // Scalar
```

