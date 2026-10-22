using LyxCsharp.Hydraulics;
using LyxCsharp.Units;

namespace LyxCsharp.Hydraulics.Tests;

public class PipeFlowTests
{
    private static readonly Pipe SteelPipe = new(0.1, 50.0, 0.000045);

    [Test]
    public void AreaOfCircularPipe()
    {
        var pipe = new Pipe(2.0, 1.0, 0.0);

        Assert.That(pipe.AreaSquareMetres, Is.EqualTo(Math.PI).Within(1e-12));
    }

    [Test]
    public void PipeFromQuantitiesConvertsToMetres()
    {
        var pipe = Pipe.From(
            new Quantity(100, UnitCatalog.Millimetre),
            new Quantity(10, UnitCatalog.Metre),
            new Quantity(0.045, UnitCatalog.Millimetre));

        Assert.Multiple(() =>
        {
            Assert.That(pipe.DiameterMetres, Is.EqualTo(0.1).Within(1e-12));
            Assert.That(pipe.LengthMetres, Is.EqualTo(10.0).Within(1e-12));
            Assert.That(pipe.RoughnessMetres, Is.EqualTo(0.000045).Within(1e-12));
        });
    }

    [TestCase(1000.0, FlowRegime.Laminar)]
    [TestCase(2299.0, FlowRegime.Laminar)]
    [TestCase(3000.0, FlowRegime.Transitional)]
    [TestCase(4000.0, FlowRegime.Turbulent)]
    [TestCase(100000.0, FlowRegime.Turbulent)]
    public void ClassifiesRegime(double reynolds, FlowRegime expected)
    {
        Assert.That(PipeFlowCalculator.Classify(reynolds), Is.EqualTo(expected));
    }

    [Test]
    public void WaterAtModerateFlowIsTurbulent()
    {
        var result = new PipeFlowCalculator().Calculate(
            SteelPipe, Fluid.Water, new Quantity(10, UnitCatalog.LitrePerSecond));

        Assert.Multiple(() =>
        {
            Assert.That(result.Regime, Is.EqualTo(FlowRegime.Turbulent));
            Assert.That(result.VelocityMetresPerSecond, Is.EqualTo(1.2732).Within(1e-3));
            Assert.That(result.PressureDropPascals, Is.GreaterThan(0));
        });
    }

    [Test]
    public void OilAtLowFlowIsLaminar()
    {
        var result = new PipeFlowCalculator().Calculate(
            SteelPipe, Fluid.Oil, new Quantity(0.1, UnitCatalog.LitrePerSecond));

        Assert.That(result.Regime, Is.EqualTo(FlowRegime.Laminar));
    }

    [Test]
    public void LaminarDropMatchesHagenPoiseuille()
    {
        var result = new PipeFlowCalculator().Calculate(
            SteelPipe, Fluid.Oil, new Quantity(0.1, UnitCatalog.LitrePerSecond));

        var expected = 128.0 * Fluid.Oil.ViscosityPascalSeconds * SteelPipe.LengthMetres * 0.0001
                       / (Math.PI * Math.Pow(SteelPipe.DiameterMetres, 4));

        Assert.That(result.PressureDropPascals, Is.EqualTo(expected).Within(expected * 1e-6));
    }

    [Test]
    public void FlowUnitDoesNotChangeResult()
    {
        var calculator = new PipeFlowCalculator();

        var a = calculator.Calculate(SteelPipe, Fluid.Water, new Quantity(6, UnitCatalog.LitrePerSecond));
        var b = calculator.Calculate(SteelPipe, Fluid.Water, new Quantity(360, UnitCatalog.LitrePerMinute));

        Assert.That(b.PressureDropPascals, Is.EqualTo(a.PressureDropPascals).Within(1e-6));
    }

    [Test]
    public void HigherFlowGivesHigherDrop()
    {
        var calculator = new PipeFlowCalculator();

        var low = calculator.Calculate(SteelPipe, Fluid.Water, new Quantity(5, UnitCatalog.LitrePerSecond));
        var high = calculator.Calculate(SteelPipe, Fluid.Water, new Quantity(15, UnitCatalog.LitrePerSecond));

        Assert.That(high.PressureDropPascals, Is.GreaterThan(low.PressureDropPascals));
    }

    [TestCase(0.0)]
    [TestCase(-1.0)]
    public void NonPositiveFlowThrows(double flow)
    {
        var calculator = new PipeFlowCalculator();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            calculator.Calculate(SteelPipe, Fluid.Water, new Quantity(flow, UnitCatalog.LitrePerSecond)));
    }

    [Test]
    public void CustomFrictionModelIsUsed()
    {
        var calculator = new PipeFlowCalculator(new ConstantFriction(0.02));

        var result = calculator.Calculate(SteelPipe, Fluid.Water, new Quantity(10, UnitCatalog.LitrePerSecond));

        Assert.That(result.FrictionFactor, Is.EqualTo(0.02));
    }

    [Test]
    [Category("Slow")]
    public void DropGrowsMonotonicallyAcrossFlowSweep()
    {
        var calculator = new PipeFlowCalculator();
        var previous = 0.0;

        for (var i = 1; i <= 100_000; i++)
        {
            var flow = new Quantity(i * 0.001, UnitCatalog.LitrePerSecond);
            var drop = calculator.Calculate(SteelPipe, Fluid.Water, flow).PressureDropPascals;
            Assert.That(drop, Is.GreaterThan(previous));
            previous = drop;
        }
    }

    private sealed class ConstantFriction(double factor) : IFrictionModel
    {
        public double FrictionFactor(double reynolds, double relativeRoughness) => factor;
    }
}
