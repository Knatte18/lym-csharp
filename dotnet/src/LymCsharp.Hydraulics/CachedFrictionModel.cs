using System.Collections.Concurrent;

namespace LymCsharp.Hydraulics;

/// <summary>
/// Remembers an inner friction model's factor for each exact Reynolds number and relative roughness at 2300 and above,
/// so a repeated flow skips the turbulent formula and returns the same value the inner model gave.
/// Below 2300 every call goes to the inner model, since the laminar factor costs less than a lookup.
/// Throws <see cref="ArgumentOutOfRangeException"/> when the Reynolds number is not positive.
/// Safe to share across threads; the cache grows by one entry per distinct input and is never evicted.
/// </summary>
public sealed class CachedFrictionModel(IFrictionModel inner) : IFrictionModel
{
    private const double LaminarLimit = 2300;

    private readonly ConcurrentDictionary<(double Reynolds, double RelativeRoughness), double> factors = new();

    /// <inheritdoc/>
    public double FrictionFactor(double reynolds, double relativeRoughness)
    {
        if (reynolds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(reynolds), "Reynolds number must be positive.");
        }

        if (reynolds < LaminarLimit)
        {
            return inner.FrictionFactor(reynolds, relativeRoughness);
        }

        return factors.GetOrAdd(
            (reynolds, relativeRoughness),
            key => inner.FrictionFactor(key.Reynolds, key.RelativeRoughness));
    }
}
