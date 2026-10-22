using LyxCsharp.Hydraulics;
using LyxCsharp.Units;

namespace LyxCsharp.Reporting;

/// <summary>Renders calculation results as text.</summary>
public static class ResultFormatter
{
    public static string Format(PipeFlowResult result, IUnit pressureUnit)
    {
        var drop = new Quantity(result.PressureDropPascals, UnitCatalog.Pascal).ConvertTo(pressureUnit);
        return $"{result.Regime} flow, v = {result.VelocityMetresPerSecond:0.###} m/s, "
               + $"Re = {result.Reynolds:0}, dP = {drop}";
    }
}
