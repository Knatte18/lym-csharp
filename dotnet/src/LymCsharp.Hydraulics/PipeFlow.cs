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

    /// <summary>
    /// Computes the Reynolds number of a fluid flowing through a pipe at the given volumetric flow.
    /// Throws <see cref="ArgumentOutOfRangeException"/> when the flow is not positive.
    /// </summary>
    public static double Reynolds(Pipe pipe, Fluid fluid, Quantity flow)
    {
        var velocity = MeanVelocityMetresPerSecond(pipe, flow);
        return fluid.DensityKgPerCubicMetre * velocity * pipe.DiameterMetres / fluid.ViscosityPascalSeconds;
    }

    public PipeFlowResult Calculate(Pipe pipe, Fluid fluid, Quantity flow)
    {
        var velocity = MeanVelocityMetresPerSecond(pipe, flow);
        var reynolds = Reynolds(pipe, fluid, flow);
        var f = friction.FrictionFactor(reynolds, pipe.RoughnessMetres / pipe.DiameterMetres);
        var drop = f * (pipe.LengthMetres / pipe.DiameterMetres)
                   * fluid.DensityKgPerCubicMetre * velocity * velocity / 2.0;

        return new PipeFlowResult(velocity, reynolds, Classify(reynolds), f, drop);
    }

    private static double MeanVelocityMetresPerSecond(Pipe pipe, Quantity flow)
    {
        var q = flow.ConvertTo(UnitCatalog.CubicMetrePerSecond).Value;
        if (q <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(flow), "Flow must be positive.");
        }

        return q / pipe.AreaSquareMetres;
    }
}
