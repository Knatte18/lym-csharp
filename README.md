# lyx-csharp

Small public C# sandbox for LoomYard.
It exercises the C# path end to end: plans in C# glyphs, code written in C#, and a gate running `dotnet test`.

- `LyxCsharp.Units`: unit conversion for pressure, length and flow.
- `LyxCsharp.Hydraulics`: pipe flow calculations built on the units library.
- `LyxCsharp.Reporting`: formatting of calculation results.

Build and test with `dotnet build` and `dotnet test`; slow tests carry `[Category("Slow")]`.
