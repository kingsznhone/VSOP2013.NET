# Requirements

.NET 10 Runtime

SIMD acceleration uses `Vector128/256/512` hardware intrinsics. `SimdMode.Auto` (default) picks the widest width the CPU accelerates, and falls back to scalar evaluation. Requesting a specific width that the CPU does not accelerate throws `PlatformNotSupportedException`.

The data file `VSOP2013.BR` is copied next to your application automatically by the NuGet package.
