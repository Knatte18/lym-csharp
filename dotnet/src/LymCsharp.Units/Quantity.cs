namespace LymCsharp.Units;

/// <summary>The physical dimension a unit measures.</summary>
public enum Dimension
{
    Pressure,
    Length,
    Flow,
}

/// <summary>A unit that converts to and from its dimension's base unit.</summary>
public interface IUnit
{
    string Symbol { get; }

    Dimension Dimension { get; }

    double ToBase(double value);

    double FromBase(double baseValue);
}

/// <summary>A unit related to the base unit by a constant factor.</summary>
public sealed record LinearUnit(string Symbol, Dimension Dimension, double Factor) : IUnit
{
    public double ToBase(double value) => value * Factor;

    public double FromBase(double baseValue) => baseValue / Factor;
}

/// <summary>A value paired with its unit.</summary>
public readonly record struct Quantity(double Value, IUnit Unit)
{
    public Quantity ConvertTo(IUnit target)
    {
        if (target.Dimension != Unit.Dimension)
        {
            throw new InvalidOperationException(
                $"Cannot convert {Unit.Dimension} to {target.Dimension}.");
        }

        return new Quantity(target.FromBase(Unit.ToBase(Value)), target);
    }

    public override string ToString() => $"{Value:0.###} {Unit.Symbol}";
}
