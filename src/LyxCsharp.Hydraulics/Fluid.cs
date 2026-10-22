namespace LyxCsharp.Hydraulics;

/// <summary>A fluid with the properties pipe flow needs.</summary>
public sealed record Fluid(string Name, double DensityKgPerCubicMetre, double ViscosityPascalSeconds)
{
    public static Fluid Water { get; } = new("water", 998.0, 0.001);

    public static Fluid Oil { get; } = new("oil", 870.0, 0.05);
}
