using LymCsharp.Hydraulics;
using LymCsharp.Reporting;
using LymCsharp.Units;

namespace LymCsharp.Hydraulics.Tests;

public class ResultFormatterTests
{
    [TestCase(1.5, 50_000, FlowRegime.Turbulent, 200_000, "Pa",
        "Turbulent flow, v = 1.5 m/s, Re = 50000, dP = 200000 Pa")]
    [TestCase(1.5, 50_000, FlowRegime.Turbulent, 200_000, "bar",
        "Turbulent flow, v = 1.5 m/s, Re = 50000, dP = 2 bar")]
    [TestCase(1.23456, 123_456, FlowRegime.Turbulent, 123_456, "Pa",
        "Turbulent flow, v = 1.235 m/s, Re = 123500, dP = 123500 Pa")]
    [TestCase(1.23456, 123_456, FlowRegime.Turbulent, 123_456, "bar",
        "Turbulent flow, v = 1.235 m/s, Re = 123500, dP = 1.235 bar")]
    [TestCase(0.000123456, 1.23456, FlowRegime.Laminar, 0.0123456, "Pa",
        "Laminar flow, v = 0.0001235 m/s, Re = 1.235, dP = 0.01235 Pa")]
    [TestCase(1.23456e-14, 1.0, FlowRegime.Laminar, 0.0000123456, "Pa",
        "Laminar flow, v = 0.00000000000001235 m/s, Re = 1, dP = 0.00001235 Pa")]
    [TestCase(1.5, 50_000, FlowRegime.Turbulent, -123_456, "Pa",
        "Turbulent flow, v = 1.5 m/s, Re = 50000, dP = -123500 Pa")]
    [TestCase(0.0, 0.0,FlowRegime.Laminar, 0.0, "Pa",
        "Laminar flow, v = 0 m/s, Re = 0, dP = 0 Pa")]
    [TestCase(0.05, 3000, FlowRegime.Transitional, 689.475729, "psi",
        "Transitional flow, v = 0.05 m/s, Re = 3000, dP = 0.1 psi (transitional: friction factor uncertain)")]
    public void FormatsEveryValueToFourSignificantDigits(
        double velocity, double reynolds, FlowRegime regime, double pressureDropPascals, string pressureSymbol,
        string expected)
    {
        var result = new PipeFlowResult(velocity, reynolds, regime, 0.02, pressureDropPascals);

        var text = ResultFormatter.Format(result, UnitRegistry.Find(pressureSymbol));

        Assert.That(text, Is.EqualTo(expected));
    }
}
