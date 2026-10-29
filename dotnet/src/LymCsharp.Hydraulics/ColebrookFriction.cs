namespace LymCsharp.Hydraulics;

/// <summary>
/// Laminar 64/Re below 2300, the Colebrook-White equation above, solved by fixed-point iteration.
/// Throws <see cref="InvalidOperationException"/> when the iteration does not converge within its cap.
/// </summary>
public sealed class ColebrookFriction : IFrictionModel
{
    private const double RelativeTolerance = 1e-12;
    private const int MaxIterations = 50;

    /// <inheritdoc/>
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

        // Iterates on 1/sqrt(f), starting from the Swamee-Jain approximation.
        var inverseSqrtFactor = -2.0 * Math.Log10(relativeRoughness / 3.7 + 5.74 / Math.Pow(reynolds, 0.9));
        for (var iteration = 0; iteration < MaxIterations; iteration++)
        {
            var next = -2.0 * Math.Log10(relativeRoughness / 3.7 + 2.51 * inverseSqrtFactor / reynolds);
            if (Math.Abs(next - inverseSqrtFactor) <= RelativeTolerance * next)
            {
                return 1.0 / (next * next);
            }

            inverseSqrtFactor = next;
        }

        throw new InvalidOperationException(
            $"Colebrook-White did not converge in {MaxIterations} iterations at Re {reynolds}, relative roughness {relativeRoughness}.");
    }
}
