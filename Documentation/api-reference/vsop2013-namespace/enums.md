# Enums

## VSOPBody <a href="#fields" id="fields"></a>

```csharp
public enum VSOPBody
```

### Fields <a href="#fields" id="fields"></a>

| Name    | Value |
| ------- | ----- |
| SUN     | 0     |
| MERCURY | 1     |
| VENUS   | 2     |
| EMB     | 3     |
| MARS    | 4     |
| JUPITER | 5     |
| SATURN  | 6     |
| URANUS  | 7     |
| NEPTUNE | 8     |
| PLUTO   | 9     |

***

## VSOPVariable <a href="#fields" id="fields"></a>

```csharp
public enum VSOPVariable
```

### Fields <a href="#fields" id="fields"></a>

| Name | Value |
| ---- | ----- |
| A    | 0     |
| L    | 1     |
| K    | 2     |
| H    | 3     |
| Q    | 4     |
| P    | 5     |

***

## TimeFrame <a href="#fields" id="fields"></a>

```csharp
public enum TimeFrame
```

### Fields <a href="#fields" id="fields"></a>

| Name | Value |
| ---- | ----- |
| UTC  | 0     |
| TAI  | 1     |
| TT   | 2     |
| TDB  | 3     |

***

## CoordinatesType

<pre class="language-csharp"><code class="lang-csharp"><strong>public enum CoordinatesType
</strong></code></pre>

### Fields <a href="#fields" id="fields"></a>

| Name        | Value |
| ----------- | ----- |
| Elliptic    | 0     |
| Rectangular | 1     |
| Spherical   | 2     |

***

## ReferenceFrame

<pre class="language-csharp"><code class="lang-csharp"><strong>public enum ReferenceFrame
</strong></code></pre>

### Fields <a href="#fields" id="fields"></a>

| Name           | Value |
| -------------- | ----- |
| DynamicalJ2000 | 0     |
| ICRSJ2000      | 2     |

***

## SimdMode

```csharp
public enum SimdMode
```

Selects the vector width used by [Calculator](calculator-class/) and [CalculatorF](calculatorf-class.md).

| Name      | Value | Description                                          |
| --------- | ----- | ---------------------------------------------------- |
| Auto      | 0     | Pick the widest hardware accelerated width available |
| Scalar    | 1     | Scalar evaluation only                               |
| Vector128 | 2     | 128-bit vectors (2 doubles / 4 floats)               |
| Vector256 | 3     | 256-bit vectors (4 doubles / 8 floats)               |
| Vector512 | 4     | 512-bit vectors (8 doubles / 16 floats)              |

