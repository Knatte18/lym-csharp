# lym-csharp

Small public C# sandbox for LoomYard.
It exercises the C# path end to end: plans in C# glyphs, code written in C#, and a gate running `dotnet test`.

- `LymCsharp.Units`: unit conversion for pressure, length and flow.
- `LymCsharp.Hydraulics`: pipe flow calculations built on the units library.
- `LymCsharp.Reporting`: formatting of calculation results.
- `LymCsharp.Regression`: a capture/compare regression harness over the other libraries.

The .NET solution lives in `dotnet/`, the hub's anchor, so LoomYard's anchor in a subdirectory is exercised as well.
Build and test there with `dotnet build` and `dotnet test`; slow tests carry `[Category("Slow")]`.

## Regression harness

`LymCsharp.Regression` runs a fixed set of hydraulics and reporting cases and checks their output against a captured baseline.
From `dotnet/`:

```
dotnet run --project src/LymCsharp.Regression -- capture <baseline_dir> [--skip-file <path>]
dotnet run --project src/LymCsharp.Regression -- compare <baseline_dir> [--skip-file <path>]
```

Capture writes each case's scrubbed output to `<baseline_dir>/<case>.txt`: timestamps and machine paths are replaced and numbers rounded to 10 significant digits.
Compare prints one `drift:` line per drifted case, naming its first deviating line with both values.
A skip file names one case per line to leave out.
Exit codes: 0 no drift, 1 drift, 2 usage error.
Use `.baseline/` as the directory for a local baseline; it is gitignored.
