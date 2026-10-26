# Quick Start

#### Install Package

```sh
PM> Install-Package VSOP2013.NET
```

#### Calculate a planet position:

```csharp
using VSOP2013;

Calculator vsop = new Calculator();

// Create VSOPTime using UTC .
DateTime Tinput = DateTime.Now;
VSOPTime vTime = new VSOPTime(Tinput.ToUniversalTime(),TimeFrame.UTC);

// Calculate EMB's present position
VSOPResult_ELL ell = vsop.GetPlanetPosition(VSOPBody.EMB, vTime);

// Convert to diffirent coordinate system.
VSOPResult_XYZ xyz=ell.ToXYZ();
VSOPResult_LBR lbr=ell.ToLBR();

// Print result
Console.WriteLine($"Body: {Enum.GetName(ell.Body)}");
Console.WriteLine($"Coordinates Type: {Enum.GetName(ell.CoordinatesType)}");
Console.WriteLine($"Coordinates Reference: {Enum.GetName(ell.CoordinatesReference)}");
Console.WriteLine($"Reference Frame: {Enum.GetName(ell.ReferenceFrame)}");
Console.WriteLine($"Time UTC: {ell.Time.UTC.ToString("o")}");
Console.WriteLine($"Time TDB: {ell.Time.TDB.ToString("o")}");
Console.WriteLine("---------------------------------------------------------------");
Console.WriteLine(String.Format("{0,-33}{1,30}", "semi-major axis (au)", ell.a));
Console.WriteLine(String.Format("{0,-33}{1,30}", "mean longitude (rad)", ell.l));
Console.WriteLine(String.Format("{0,-33}{1,30}", "k = e*cos(pi) (rad)", ell.k));
Console.WriteLine(String.Format("{0,-33}{1,30}", "h = e*sin(pi) (rad)", ell.h));
Console.WriteLine(String.Format("{0,-33}{1,30}", "q = sin(i/2)*cos(omega) (rad)", ell.q));
Console.WriteLine(String.Format("{0,-33}{1,30}", "p = sin(i/2)*sin(omega) (rad)", ell.p));
Console.WriteLine("===============================================================");
```

#### Choose SIMD mode and truncation

`Calculator` uses the widest hardware accelerated vector width by default. You can pick one explicitly, and drop small terms to trade precision for speed:

```csharp
// Full series, widest SIMD width (default).
Calculator full = new Calculator();
Console.WriteLine(full.Mode); // e.g. Vector512 / Vector256 / Scalar

// Scalar evaluation, terms smaller than 1e-14 are dropped.
Calculator scalar = new Calculator(SimdMode.Scalar, truncation: 1e-14);
```

#### Fast, low precision calculation

For visualization or animation, use [CalculatorF](../api-reference/vsop2013-namespace/calculatorf-class.md). It runs in single precision and is much faster, but is not suitable for precise astronomy.

```csharp
CalculatorF fast = new CalculatorF();
VSOPResult_ELL ell = fast.GetPlanetPosition(VSOPBody.MARS, vTime);
```

> **Note:** Create a calculator once and reuse it. Construction loads the data file, and instances are thread safe after that.
