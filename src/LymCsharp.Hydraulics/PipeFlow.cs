using LymCsharp.Units;

namespace LymCsharp.Hydraulics;

/// <summary>Result of a pipe flow calculation.</summary>
public sealed record PipeFlowResult(
    double VelocityMetresPerSecond,
    double Reynolds,
    FlowRegime Regime,
    double FrictionFactor,
    double PressureDropPascals);

/// <summary>Pressure drop and regime for a fluid flowing through a pipe.</summary>
public sealed class PipeFlowCalculator(IFrictionModel friction)
{
    public PipeFlowCalculator() : this(new SwameeJainFriction())
    {
    }

    public static FlowRegime Classify(double reynolds) => reynolds switch
    {
        < 2300 => FlowRegime.Laminar,
        < 4000 => FlowRegime.Transitional,
        _ => FlowRegime.Turbulent,
    };

    public PipeFlowResult Calculate(Pipe pipe, Fluid fluid, Quantity flow)
    {
        var q = flow.ConvertTo(UnitCatalog.CubicMetrePerSecond).Value;
        if (q <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(flow), "Flow must be positive.");
        }

        var velocity = q / pipe.AreaSquareMetres;
        var reynolds = fluid.DensityKgPerCubicMetre * velocity * pipe.DiameterMetres / fluid.ViscosityPascalSeconds;
        var f = friction.FrictionFactor(reynolds, pipe.RoughnessMetres / pipe.DiameterMetres);
        var drop = f * (pipe.LengthMetres / pipe.DiameterMetres)
                   * fluid.DensityKgPerCubicMetre * velocity * velocity / 2.0;

        return new PipeFlowResult(velocity, reynolds, Classify(reynolds), f, drop);
    }
}
