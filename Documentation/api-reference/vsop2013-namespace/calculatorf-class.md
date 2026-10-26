# CalculatorF Class

## Definition

Fast, low precision approximation of VSOP2013, intended for visualization, animation and teaching.

```csharp
public class CalculatorF
```

Every step (time, coefficients, phase, sin/cos, accumulation) runs in single precision. Combined with the default truncation, it evaluates roughly 100 to 500 times faster than [Calculator](calculator-class/).

An instance is read-only after construction and safe to share across threads.

> **Warning:** Do not use it for precise astronomy (telescope pointing, eclipse or transit prediction). Use [Calculator](calculator-class/) when accuracy matters.

## Accuracy

Typical error over ±1000 years around J2000:

| Variable   | Typical maximum error                  |
| ---------- | -------------------------------------- |
| L          | ~3e-3 rad (~640 arcsec, Mercury)       |
| A          | ~4e-5 au (Pluto)                       |
| K, H, Q, P | ~1e-6                                  |

Within ±100 years, L stays below ~70 arcsec. The error is dominated by the float rounding of the mean longitude, not by truncation.

The official tables assume \|t\| ≤ 1 millennium. Beyond that, high powers of t amplify the truncation error.

## Constructors <a href="#constructors" id="constructors"></a>

```csharp
public CalculatorF();
public CalculatorF(SimdMode mode, double truncation = CalculatorF.DefaultTruncation);
```

**Parameters**

`mode` [SimdMode](enums.md#simdmode)

Vector width. `SimdMode.Auto` picks the widest hardware accelerated width.

`truncation` double

Truncation level ρ. Terms with `sqrt(C² + S²) < ρ` are dropped when the data is loaded. Defaults to `DefaultTruncation` (1e-8), which keeps about 2% of the series. `0` keeps the full series, which is much slower and gains little because float precision is the limiting factor.

**Exceptions**

`PlatformNotSupportedException`: the requested vector width is not hardware accelerated.

`ArgumentOutOfRangeException`: `truncation` is negative or not finite.

## Fields and Properties

`const double DefaultTruncation`

1e-8. A float carries about 7 significant digits, so smaller terms are lost in rounding noise anyway.

`Mode` [SimdMode](enums.md#simdmode)

Vector width actually used after hardware detection.

`Truncation` double

Truncation level applied to this instance.

## Methods <a href="#methods" id="methods"></a>

```csharp
public VSOPResult_ELL GetPlanetPosition(VSOPBody body, VSOPTime time)
public ValueTask<VSOPResult_ELL> GetPlanetPositionAsync(VSOPBody body, VSOPTime time)
public float GetVariable(VSOPBody body, VSOPVariable variable, VSOPTime time)
public ValueTask<float> GetVariableAsync(VSOPBody body, VSOPVariable variable, VSOPTime time)
```

Same semantics as [Calculator](calculator-class/), with these differences:

* `GetVariable` returns `float`.
* `GetPlanetPosition` runs sequentially on the calling thread. Values in the returned [VSOPResult\_ELL](vsopresult-class/vsopresult_ell-class.md) are widened from float.

## Example

```csharp
CalculatorF fast = new CalculatorF();
VSOPTime time = new VSOPTime(DateTime.UtcNow, TimeFrame.UTC);

VSOPResult_ELL ell = fast.GetPlanetPosition(VSOPBody.MARS, time);
VSOPResult_XYZ xyz = ell.ToXYZ();
```
