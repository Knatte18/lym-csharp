using LymCsharp.Units;

namespace LymCsharp.Hydraulics;

/// <summary>A circular pipe section.</summary>
public sealed record Pipe(double DiameterMetres, double LengthMetres, double RoughnessMetres)
{
    public double AreaSquareMetres => Math.PI * DiameterMetres * DiameterMetres / 4.0;

    public static Pipe From(Quantity diameter, Quantity length, Quantity roughness)
    {
        return new Pipe(
            diameter.ConvertTo(UnitCatalog.Metre).Value,
            length.ConvertTo(UnitCatalog.Metre).Value,
            roughness.ConvertTo(UnitCatalog.Metre).Value);
    }
}
