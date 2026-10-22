namespace LyxCsharp.Hydraulics;

/// <summary>Laminar 64/Re below 2300, the Swamee-Jain approximation above.</summary>
public sealed class SwameeJainFriction : IFrictionModel
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

        var term = Math.Log10(relativeRoughness / 3.7 + 5.74 / Math.Pow(reynolds, 0.9));
        return 0.25 / (term * term);
    }
}
