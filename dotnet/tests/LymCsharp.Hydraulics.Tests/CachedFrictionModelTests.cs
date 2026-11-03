using LymCsharp.Hydraulics;

namespace LymCsharp.Hydraulics.Tests;

public class CachedFrictionModelTests
{
    [TestCase(2300.0)]
    [TestCase(100_000.0)]
    public void RepeatedTurbulentCallReturnsCachedFactorWithoutReachingInnerModel(double reynolds)
    {
        var inner = new CountingFriction();
        var model = new CachedFrictionModel(inner);

        var first = model.FrictionFactor(reynolds, 0.001);
        var second = model.FrictionFactor(reynolds, 0.001);

        Assert.Multiple(() =>
        {
            Assert.That(inner.Calls, Is.EqualTo(1));
            Assert.That(first, Is.EqualTo(CountingFriction.Factor(reynolds, 0.001)));
            Assert.That(second, Is.EqualTo(first));
        });
    }

    [Test]
    public void DistinctTurbulentInputsEachReachInnerModelAndKeepTheirOwnFactor()
    {
        var inner = new CountingFriction();
        var model = new CachedFrictionModel(inner);

        var baseline = model.FrictionFactor(10_000.0, 0.001);
        var otherReynolds = model.FrictionFactor(10_000.5, 0.001);
        var otherRoughness = model.FrictionFactor(10_000.0, 0.002);

        Assert.Multiple(() =>
        {
            Assert.That(inner.Calls, Is.EqualTo(3));
            Assert.That(baseline, Is.EqualTo(CountingFriction.Factor(10_000.0, 0.001)));
            Assert.That(otherReynolds, Is.EqualTo(CountingFriction.Factor(10_000.5, 0.001)));
            Assert.That(otherRoughness, Is.EqualTo(CountingFriction.Factor(10_000.0, 0.002)));
        });
    }

    [TestCase(1000.0)]
    [TestCase(2299.9)]
    public void LaminarCallAlwaysReachesInnerModel(double reynolds)
    {
        var inner = new CountingFriction();
        var model = new CachedFrictionModel(inner);

        model.FrictionFactor(reynolds, 0.001);
        var second = model.FrictionFactor(reynolds, 0.001);

        Assert.Multiple(() =>
        {
            Assert.That(inner.Calls, Is.EqualTo(2));
            Assert.That(second, Is.EqualTo(CountingFriction.Factor(reynolds, 0.001)));
        });
    }

    [TestCase(0.0)]
    [TestCase(-5.0)]
    public void NonPositiveReynoldsThrowsWithoutReachingInnerModel(double reynolds)
    {
        var inner = new CountingFriction();
        var model = new CachedFrictionModel(inner);

        Assert.Throws<ArgumentOutOfRangeException>(() => model.FrictionFactor(reynolds, 0.001));
        Assert.That(inner.Calls, Is.EqualTo(0));
    }

    private sealed class CountingFriction : IFrictionModel
    {
        public int Calls { get; private set; }

        public static double Factor(double reynolds, double relativeRoughness) =>
            1.0 / reynolds + relativeRoughness;

        public double FrictionFactor(double reynolds, double relativeRoughness)
        {
            Calls++;
            return Factor(reynolds, relativeRoughness);
        }
    }
}
