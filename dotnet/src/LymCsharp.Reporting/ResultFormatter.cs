using LymCsharp.Hydraulics;
using LymCsharp.Units;

namespace LymCsharp.Reporting;

/// <summary>Renders calculation results as text.</summary>
public static class ResultFormatter
{
    private const int SignificantDigits = 4;

    /// <summary>
    /// Renders the flow regime, velocity, Reynolds number and pressure drop in <paramref name="pressureUnit"/> as one line.
    /// Every numeric value is rounded to 4 significant digits and printed without trailing zeros.
    /// </summary>
    public static string Format(PipeFlowResult result, IUnit pressureUnit)
    {
        var drop = new Quantity(result.PressureDropPascals, UnitCatalog.Pascal).ConvertTo(pressureUnit);
        return $"{result.Regime} flow, v = {RoundToSignificantDigits(result.VelocityMetresPerSecond)} m/s, "
               + $"Re = {RoundToSignificantDigits(result.Reynolds)}, "
               + $"dP = {RoundToSignificantDigits(drop.Value)} {drop.Unit.Symbol}";
    }

    private static double RoundToSignificantDigits(double value)
    {
        if (value == 0)
        {
            return 0;
        }

        var leadingDigitExponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
        var decimals = SignificantDigits - 1 - leadingDigitExponent;
        if (decimals >= 0)
        {
            // Math.Round accepts at most 15 decimals.
            return Math.Round(value, Math.Min(decimals, 15));
        }

        // Math.Round takes no negative decimals, so round to a whole power of ten instead;
        // the result stays an exact integer.
        var scale = Math.Pow(10, -decimals);
        return Math.Round(value / scale) * scale;
    }
}
