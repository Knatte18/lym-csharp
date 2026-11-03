using System.Globalization;

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

    /// <summary>
    /// Reads a quantity from a number, one space and a unit symbol, the form <see cref="ToString"/> writes.
    /// The number is read in the current culture, as <see cref="ToString"/> writes it.
    /// </summary>
    /// <exception cref="FormatException">The text holds no space, or its number part is not a number.</exception>
    /// <exception cref="KeyNotFoundException">The unit symbol names no known unit.</exception>
    public static Quantity Parse(string text)
    {
        var separator = text.IndexOf(' ');
        if (separator < 0)
        {
            throw new FormatException($"Quantity '{text}' has no space between value and unit.");
        }

        var value = double.Parse(text[..separator], NumberStyles.Float, CultureInfo.CurrentCulture);
        var unit = UnitRegistry.Find(text[(separator + 1)..]);
        return new Quantity(value, unit);
    }
}
