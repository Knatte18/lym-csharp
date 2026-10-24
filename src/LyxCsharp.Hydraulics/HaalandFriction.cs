namespace LyxCsharp.Hydraulics;

/// <summary>Laminar 64/Re below 2300, the Haaland explicit formula above.</summary>
public sealed class HaalandFriction : IFrictionModel
{
    public double FrictionFactor(double reynolds, double relativeRoughness)
    {
        if (reynolds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reynolds), "Reynolds number must be positive.");
        }

        if (reynolds < 2300)
        {
            return 64.0 / reynolds;
        }

        var inverseSqrtFactor = -1.8 * Math.Log10(Math.Pow(relativeRoughness / 3.7, 1.11) + 6.9 / reynolds);
        return 1.0 / (inverseSqrtFactor * inverseSqrtFactor);
    }
}
