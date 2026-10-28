using LymCsharp.Hydraulics;
using LymCsharp.Reporting;
using LymCsharp.Units;

namespace LymCsharp.Regression;

/// <summary>A named calculation whose rendered lines are captured into a baseline file.</summary>
public sealed record RegressionCase(string Name, Func<IReadOnlyList<string>> Render);

/// <summary>The fixed regression cases: pipe flow calculations rendered as raw values and a formatted report.</summary>
public static class RegressionCases
{
    private static readonly Pipe SteelDn100 = Pipe.From(
        new Quantity(100, UnitCatalog.Millimetre),
        new Quantity(50, UnitCatalog.Metre),
        new Quantity(0.045, UnitCatalog.Millimetre));

    private static readonly Pipe Pvc2Inch = Pipe.From(
        new Quantity(2, UnitCatalog.Inch),
        new Quantity(100, UnitCatalog.Foot),
        new Quantity(0.0015, UnitCatalog.Millimetre));

    private static readonly Pipe Steel6Inch = Pipe.From(
        new Quantity(6, UnitCatalog.Inch),
        new Quantity(1000, UnitCatalog.Foot),
        new Quantity(0.045, UnitCatalog.Millimetre));

    public static IReadOnlyList<RegressionCase> All { get; } =
    [
        PipeFlowCase(
            "water-steel-dn100-swamee-jain", new SwameeJainFriction(), SteelDn100, Fluid.Water,
            new Quantity(10, UnitCatalog.LitrePerSecond), UnitCatalog.Pascal),
        PipeFlowCase(
            "water-steel-dn100-haaland", new HaalandFriction(), SteelDn100, Fluid.Water,
            new Quantity(10, UnitCatalog.LitrePerSecond), UnitCatalog.Bar),
        PipeFlowCase(
            "oil-steel-dn100-laminar", new SwameeJainFriction(), SteelDn100, Fluid.Oil,
            new Quantity(0.1, UnitCatalog.LitrePerSecond), UnitCatalog.Kilopascal),
        PipeFlowCase(
            "water-pvc-2in-transitional", new SwameeJainFriction(), Pvc2Inch, Fluid.Water,
            new Quantity(7.2, UnitCatalog.LitrePerMinute), UnitCatalog.Psi),
        PipeFlowCase(
            "oil-steel-6in-turbulent", new HaalandFriction(), Steel6Inch, Fluid.Oil,
            new Quantity(36.5, UnitCatalog.LitrePerSecond), UnitCatalog.Bar),
    ];

    private static RegressionCase PipeFlowCase(
        string name, IFrictionModel friction, Pipe pipe, Fluid fluid, Quantity flow, IUnit pressureUnit) =>
        new(name, () =>
        {
            var result = new PipeFlowCalculator(friction).Calculate(pipe, fluid, flow);
            return
            [
                $"fluid = {fluid.Name}",
                $"friction_model = {friction.GetType().Name}",
                $"diameter_m = {pipe.DiameterMetres:R}",
                $"length_m = {pipe.LengthMetres:R}",
                $"roughness_m = {pipe.RoughnessMetres:R}",
                $"flow = {flow}",
                $"velocity_m_per_s = {result.VelocityMetresPerSecond:R}",
                $"reynolds = {result.Reynolds:R}",
                $"regime = {result.Regime}",
                $"friction_factor = {result.FrictionFactor:R}",
                $"pressure_drop_pa = {result.PressureDropPascals:R}",
                $"report = {ResultFormatter.Format(result, pressureUnit)}",
            ];
        });
}
