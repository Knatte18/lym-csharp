using LymCsharp.Units;

namespace LymCsharp.Units.Tests;

public class QuantityTests
{
    [TestCase(1.0, 100.0)]
    [TestCase(2.5, 250.0)]
    [TestCase(0.0, 0.0)]
    public void BarToKilopascal(double bar, double expectedKpa)
    {
        var result = new Quantity(bar, UnitCatalog.Bar).ConvertTo(UnitCatalog.Kilopascal);

        Assert.That(result.Value, Is.EqualTo(expectedKpa).Within(1e-9));
    }

    [Test]
    public void PsiToPascal()
    {
        var result = new Quantity(1.0, UnitCatalog.Psi).ConvertTo(UnitCatalog.Pascal);

        Assert.That(result.Value, Is.EqualTo(6894.757293168).Within(1e-6));
    }

    [Test]
    public void FootToMillimetre()
    {
        var result = new Quantity(1.0, UnitCatalog.Foot).ConvertTo(UnitCatalog.Millimetre);

        Assert.That(result.Value, Is.EqualTo(304.8).Within(1e-9));
    }

    [Test]
    public void InchToMetre()
    {
        var result = new Quantity(10.0, UnitCatalog.Inch).ConvertTo(UnitCatalog.Metre);

        Assert.That(result.Value, Is.EqualTo(0.254).Within(1e-12));
    }

    [Test]
    public void LitrePerMinuteToLitrePerSecond()
    {
        var result = new Quantity(60.0, UnitCatalog.LitrePerMinute).ConvertTo(UnitCatalog.LitrePerSecond);

        Assert.That(result.Value, Is.EqualTo(1.0).Within(1e-9));
    }

    [Test]
    public void ConvertingAcrossDimensionsThrows()
    {
        var quantity = new Quantity(1.0, UnitCatalog.Bar);

        Assert.Throws<InvalidOperationException>(() => quantity.ConvertTo(UnitCatalog.Metre));
    }

    [Test]
    public void RoundTripKeepsValue()
    {
        var start = new Quantity(3.7, UnitCatalog.Psi);

        var back = start.ConvertTo(UnitCatalog.Bar).ConvertTo(UnitCatalog.Psi);

        Assert.That(back.Value, Is.EqualTo(3.7).Within(1e-9));
    }

    [Test]
    public void ToStringShowsValueAndSymbol()
    {
        Assert.That(new Quantity(1.5, UnitCatalog.Bar).ToString(), Is.EqualTo("1.5 bar"));
    }

    [Test]
    public void LinearUnitConvertsToAndFromBase()
    {
        var unit = new LinearUnit("x", Dimension.Length, 4.0);

        Assert.Multiple(() =>
        {
            Assert.That(unit.ToBase(2.0), Is.EqualTo(8.0));
            Assert.That(unit.FromBase(8.0), Is.EqualTo(2.0));
        });
    }
}
