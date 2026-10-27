namespace LymCsharp.Units;

/// <summary>The built-in units. Bases: pascal, metre, cubic metre per second.</summary>
public static class UnitCatalog
{
    public static readonly IUnit Pascal = new LinearUnit("Pa", Dimension.Pressure, 1.0);
    public static readonly IUnit Kilopascal = new LinearUnit("kPa", Dimension.Pressure, 1000.0);
    public static readonly IUnit Bar = new LinearUnit("bar", Dimension.Pressure, 100_000.0);
    public static readonly IUnit Psi = new LinearUnit("psi", Dimension.Pressure, 6894.757293168);

    public static readonly IUnit Metre = new LinearUnit("m", Dimension.Length, 1.0);
    public static readonly IUnit Millimetre = new LinearUnit("mm", Dimension.Length, 0.001);
    public static readonly IUnit Foot = new LinearUnit("ft", Dimension.Length, 0.3048);
    public static readonly IUnit Inch = new LinearUnit("in", Dimension.Length, 0.0254);

    public static readonly IUnit CubicMetrePerSecond = new LinearUnit("m3/s", Dimension.Flow, 1.0);
    public static readonly IUnit LitrePerSecond = new LinearUnit("l/s", Dimension.Flow, 0.001);
    public static readonly IUnit LitrePerMinute = new LinearUnit("l/min", Dimension.Flow, 0.001 / 60.0);

    public static IReadOnlyList<IUnit> All { get; } =
    [
        Pascal, Kilopascal, Bar, Psi,
        Metre, Millimetre, Foot, Inch,
        CubicMetrePerSecond, LitrePerSecond, LitrePerMinute,
    ];
}
