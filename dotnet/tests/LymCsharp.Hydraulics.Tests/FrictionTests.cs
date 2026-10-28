using LymCsharp.Hydraulics;

namespace LymCsharp.Hydraulics.Tests;

public class FrictionTests
{
    private readonly SwameeJainFriction _model = new();

    [TestCase(500.0)]
    [TestCase(1000.0)]
    [TestCase(2000.0)]
    public void LaminarFactorIs64OverRe(double reynolds)
    {
        Assert.That(_model.FrictionFactor(reynolds, 0.001), Is.EqualTo(64.0 / reynolds).Within(1e-12));
    }

    [Test]
    public void SmoothTurbulentFactorIsInExpectedRange()
    {
        var factor = _model.FrictionFactor(100_000, 0.0);

        Assert.That(factor, Is.InRange(0.017, 0.020));
    }

    [Test]
    public void RoughnessRaisesFactor()
    {
        var smooth = _model.FrictionFactor(100_000, 0.0001);
        var rough = _model.FrictionFactor(100_000, 0.01);

        Assert.That(rough, Is.GreaterThan(smooth));
    }

    [TestCase(0.0)]
    [TestCase(-5.0)]
    public void NonPositiveReynoldsThrows(double reynolds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _model.FrictionFactor(reynolds, 0.001));
    }

    [Test]
    public void WaterIsLessViscousThanOil()
    {
        Assert.That(Fluid.Water.ViscosityPascalSeconds, Is.LessThan(Fluid.Oil.ViscosityPascalSeconds));
    }
}
