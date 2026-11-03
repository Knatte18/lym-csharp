namespace LymCsharp.Hydraulics;

/// <summary>
/// Churchill's 1977 equation: one explicit formula for laminar, transitional and turbulent flow, with no regime switch.
/// It reduces to 64/Re at low Reynolds numbers and approximates Colebrook-White at high ones.
/// </summary>
public sealed class ChurchillFriction : IFrictionModel
{
    /// <inheritdoc/>
    public double FrictionFactor(double reynolds, double relativeRoughness)
    {
        if (reynolds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reynolds), "Reynolds number must be positive.");
        }

        var laminarTerm = Math.Pow(8.0 / reynolds, 12);
        var roughTurbulentTerm = Math.Pow(
            2.457 * Math.Log(1.0 / (Math.Pow(7.0 / reynolds, 0.9) + 0.27 * relativeRoughness)), 16);
        var transitionTerm = Math.Pow(37530.0 / reynolds, 16);

        return 8.0 * Math.Pow(laminarTerm + Math.Pow(roughTurbulentTerm + transitionTerm, -1.5), 1.0 / 12);
    }
}
