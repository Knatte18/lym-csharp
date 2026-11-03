using System.Globalization;
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
    /// A transitional flow's line ends with the note " (transitional: friction factor uncertain)",
    /// because no friction model is reliable between laminar and turbulent flow.
    /// </summary>
    public static string Format(PipeFlowResult result, IUnit pressureUnit)
    {
        var drop = new Quantity(result.PressureDropPascals, UnitCatalog.Pascal).ConvertTo(pressureUnit);
        var line = $"{result.Regime} flow, v = {FormatToSignificantDigits(result.VelocityMetresPerSecond)} m/s, "
                   + $"Re = {FormatToSignificantDigits(result.Reynolds)}, "
                   + $"dP = {FormatToSignificantDigits(drop.Value)} {drop.Unit.Symbol}";
        return result.Regime == FlowRegime.Transitional
            ? line + " (transitional: friction factor uncertain)"
            : line;
    }

    // Fixed-point text, because interpolating a double prints exponent notation below 1e-4.
    private static string FormatToSignificantDigits(double value)
    {
        if (value == 0)
        {
            return "0";
        }

        var leadingDigitExponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
        var decimals = SignificantDigits - 1 - leadingDigitExponent;
        var rounded = RoundToDecimals(value, decimals);
        return rounded.ToString("0." + new string('#', Math.Max(decimals, 0)), CultureInfo.InvariantCulture);
    }

    private static double RoundToDecimals(double value, int decimals)
    {
        // Math.Round accepts only 0 to 15 decimals, so the other cases round by scaling by a power of ten.
        if (decimals < 0)
        {
            var scale = Math.Pow(10, -decimals);
            return Math.Round(value / scale) * scale;
        }

        if (decimals > 15)
        {
            var scale = Math.Pow(10, decimals);
            return Math.Round(value * scale) / scale;
        }

        return Math.Round(value, decimals);
    }
}
