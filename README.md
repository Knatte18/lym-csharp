# lym-csharp

Small public C# sandbox for LoomYard.
It exercises the C# path end to end: plans in C# glyphs, code written in C#, and a gate running `dotnet test`.

- `LymCsharp.Units`: unit conversion for pressure, length and flow.
- `LymCsharp.Hydraulics`: pipe flow calculations built on the units library.
- `LymCsharp.Reporting`: formatting of calculation results.

The .NET solution lives in `dotnet/`, the hub's anchor, so LoomYard's anchor in a subdirectory is exercised as well.
Build and test there with `dotnet build` and `dotnet test`; slow tests carry `[Category("Slow")]`.
