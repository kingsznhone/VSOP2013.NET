# Performance

## Environment

```
BenchmarkDotNet v0.15.8, Windows 11 (10.0.26300.9711)
12th Gen Intel Core i9-12950HX 2.30GHz, 1 CPU, 16 logical and 16 physical cores
.NET SDK 10.0.402
.NET 10.0.13 (10.0.13, 10.0.1326.50504), X64 RyuJIT x86-64-v3
```

## Benchmark

`GetPlanetPosition(VSOPBody.EMB, time)` for every combination of vector width and precision.

* **Double**: [Calculator](../api-reference/vsop2013-namespace/calculator-class/) with the full series.
* **Float**: [CalculatorF](../api-reference/vsop2013-namespace/calculatorf-class.md) with the default truncation (1e-8).

Ratio is relative to **Double** in the same SIMD mode.

| Method | Mode      |         Mean |    Error |   StdDev | Ratio | Allocated |
| ------ | --------- | -----------: | -------: | -------: | ----: | --------: |
| Double | Scalar    | 1,241.023 μs | 24.73 μs | 21.92 μs |  1.00 |    3167 B |
| Float  | Scalar    |    12.644 μs |  0.19 μs |  0.18 μs |  0.01 |     112 B |
| Double | Vector256 |   242.563 μs |  4.81 μs |  4.02 μs | 1.000 |    3140 B |
| Float  | Vector256 |     2.410 μs |  0.05 μs |  0.04 μs | 0.010 |     112 B |

## Summary

| Comparison                                   | Speed up |
| -------------------------------------------- | -------: |
| Double: Vector256 vs Scalar                  |    ~5.1x |
| Float: Vector256 vs Scalar                   |    ~5.2x |
| Float vs Double (Scalar)                     |     ~98x |
| Float vs Double (Vector256)                  |    ~101x |
| Float Vector256 vs Double Scalar (best case) |    ~515x |

## Notes

* `Calculator.GetPlanetPosition` evaluates the six variables in parallel, while `CalculatorF.GetPlanetPosition` runs sequentially on the calling thread. The Double figures above already include that parallelism.
* The Float speed up comes from two sources: single precision arithmetic (twice as many lanes per vector) and the default truncation, which keeps only about 2% of the series. See [CalculatorF](../api-reference/vsop2013-namespace/calculatorf-class.md) for its accuracy.
* `Calculator` accepts a `truncation` parameter as well. Use it to trade precision for speed while staying in double precision.
* Results depend on the CPU. `Vector256` requires AVX2. `SimdMode.Auto` selects the widest available width.

The benchmark source is [PerfTest.cs](https://github.com/kingsznhone/VSOP2013.NET/blob/main/Demo/PerfTest.cs) in the `Demo` project.
