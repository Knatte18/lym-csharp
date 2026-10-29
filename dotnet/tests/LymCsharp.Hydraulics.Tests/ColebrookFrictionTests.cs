using LymCsharp.Hydraulics;

namespace LymCsharp.Hydraulics.Tests;

public class ColebrookFrictionTests
{
    private readonly ColebrookFriction _model = new();

    [TestCase(500.0)]
    [TestCase(1000.0)]
    [TestCase(2000.0)]
    public void LaminarFactorIs64OverRe(double reynolds)
    {
        Assert.That(_model.FrictionFactor(reynolds, 0.001), Is.EqualTo(64.0 / reynolds).Within(1e-12));
    }

    [TestCase(0.0)]
    [TestCase(-5.0)]
    public void NonPositiveReynoldsThrows(double reynolds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _model.FrictionFactor(reynolds, 0.001));
    }

    [Test]
    public void NonConvergingInputThrows()
    {
        Assert.Throws<InvalidOperationException>(() => _model.FrictionFactor(double.NaN, 0.001));
    }

    [TestCase(4_000.0, 0.0)]
    [TestCase(100_000.0, 0.0)]
    [TestCase(100_000.0, 0.001)]
    [TestCase(10_000_000.0, 0.05)]
    public void TurbulentFactorSolvesColebrookWhite(double reynolds, double relativeRoughness)
    {
        var factor = _model.FrictionFactor(reynolds, relativeRoughness);

        Assert.That(ColebrookResidual(factor, reynolds, relativeRoughness), Is.EqualTo(0.0).Within(1e-9));
    }

    [TestCase(10_000.0, 0.0)]
    [TestCase(100_000.0, 0.0001)]
    [TestCase(1_000_000.0, 0.001)]
    [TestCase(10_000_000.0, 0.01)]
    public void TurbulentFactorAgreesWithExplicitModelsWithinThreePercent(double reynolds, double relativeRoughness)
    {
        var colebrook = _model.FrictionFactor(reynolds, relativeRoughness);
        var haaland = new HaalandFriction().FrictionFactor(reynolds, relativeRoughness);
        var swameeJain = new SwameeJainFriction().FrictionFactor(reynolds, relativeRoughness);

        Assert.Multiple(() =>
        {
            Assert.That(haaland, Is.EqualTo(colebrook).Within(3).Percent);
            Assert.That(swameeJain, Is.EqualTo(colebrook).Within(3).Percent);
        });
    }

    [Test]
    [Category("Slow")]
    public void ConvergesAcrossReynoldsAndRoughnessGrid()
    {
        var roughnesses = new[] { 0.0, 1e-6, 1e-5, 1e-4, 1e-3, 1e-2, 0.05 };

        const int steps = 440;
        var firstExponent = Math.Log10(4000);

        for (var step = 0; step <= steps; step++)
        {
            var exponent = firstExponent + step * (8.0 - firstExponent) / steps;
            var reynolds = Math.Pow(10, exponent);
            foreach (var relativeRoughness in roughnesses)
            {
                var factor = _model.FrictionFactor(reynolds, relativeRoughness);
                Assert.That(ColebrookResidual(factor, reynolds, relativeRoughness), Is.EqualTo(0.0).Within(1e-9));
            }
        }
    }

    private static double ColebrookResidual(double factor, double reynolds, double relativeRoughness)
    {
        var inverseSqrtFactor = 1.0 / Math.Sqrt(factor);
        return inverseSqrtFactor
               + 2.0 * Math.Log10(relativeRoughness / 3.7 + 2.51 * inverseSqrtFactor / reynolds);
    }
}
