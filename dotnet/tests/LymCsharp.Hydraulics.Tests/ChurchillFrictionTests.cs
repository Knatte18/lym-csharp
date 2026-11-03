using LymCsharp.Hydraulics;

namespace LymCsharp.Hydraulics.Tests;

public class ChurchillFrictionTests
{
    private readonly ChurchillFriction _model = new();

    [TestCase(100.0)]
    [TestCase(500.0)]
    [TestCase(1000.0)]
    public void LaminarFactorIs64OverRe(double reynolds)
    {
        Assert.That(_model.FrictionFactor(reynolds, 0.001), Is.EqualTo(64.0 / reynolds).Within(1e-9).Percent);
    }

    [TestCase(0.0)]
    [TestCase(-5.0)]
    public void NonPositiveReynoldsThrows(double reynolds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _model.FrictionFactor(reynolds, 0.001));
    }

    [Test]
    public void TransitionalFactorMatchesReferenceValue()
    {
        var factor = _model.FrictionFactor(3000.0, 0.0001);

        Assert.That(factor, Is.EqualTo(0.04305).Within(1).Percent);
    }

    [TestCase(10_000.0, 0.0)]
    [TestCase(100_000.0, 0.0001)]
    [TestCase(100_000.0, 0.001)]
    [TestCase(1_000_000.0, 0.001)]
    [TestCase(10_000_000.0, 0.01)]
    public void TurbulentFactorAgreesWithColebrookWithinOnePercent(double reynolds, double relativeRoughness)
    {
        var colebrook = new ColebrookFriction().FrictionFactor(reynolds, relativeRoughness);

        Assert.That(_model.FrictionFactor(reynolds, relativeRoughness), Is.EqualTo(colebrook).Within(1).Percent);
    }
}
