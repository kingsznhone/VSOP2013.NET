# Change Logs

### v1.4.0 2025.01.02

Remove native Accelerator due to fast fp error in MSVC

[https://developercommunity.visualstudio.com/t/MSVCs-sincos-implementation-is-incorrec/10582378](https://developercommunity.visualstudio.com/t/MSVCs-sincos-implementation-is-incorrec/10582378)

Use seperate sin cos calculate in calculator core, which will cause performance regression

[https://github.com/dotnet/runtime/issues/111016](https://github.com/dotnet/runtime/issues/111016)

### v1.3.1 2024.09.19

Upgrade dependency FastLZMA2 = 1.0.0

### v1.3.0 2024.08.09

Breaking Change Warning

End of .NET 6/7 support

Change data serializer from messagepack to memorypack.

Change data compressor from brotli to FastLZMA2 (beta)

Add OSplatform protection.

### v1.2.1  2024.05.04&#x20;

Fix .NET 8 performance regression.

Use P/Invoke source generator. Native accelerate speed up 50%.

### v1.2.0  2024.04.24&#x20;

Add .NET 8. which cause performance regression.

Add Native Accelerate method `GetPlanetPosition_Native` to accelerate calculation. with 60%+ performance Improvment. (Experimental)

Native CPP code is only for Windows x64 AVX2 enviroment.

Using fast floating-point compile options on native libraries can result in a decrease in precision and is difficult to estimate.

### v1.1.8  2024.01.14&#x20;

Fix critical error in ELL to XYZ convertion.

### v1.1.7  2023.12.13&#x20;

Bug fix.

Add explicit cast of VSOPResult

### v1.1.6  2023.9.3&#x20;

SIMD Accel Utility.

Bug Fix

### v1.1.5  2023.7.7&#x20;

Bug Fix.

API change with some feature in VSOP87

### v1.1.2  2023.7.05&#x20;

Many improvements.

Some of them are from VSOP87.NET

### v1.1.1  2023.03.26&#x20;

Initialize performance upgrade.

Add result classes.

Use MessagePack to compress original data.

### v1.0.0  2022.06.02&#x20;

New features.

Performance Optimization.

Upgrade to .NET 6

### v0.9b  2020.11.14&#x20;

Upgrade to .NET 5

### v0.1b  2020.07.06&#x20;

Initial PR.

\
