using LymCsharp.Units;

namespace LymCsharp.Units.Tests;

[SetCulture("")]
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

    private static IEnumerable<TestCaseData> ParseCases()
    {
        yield return new TestCaseData("1.5 bar", new Quantity(1.5, UnitCatalog.Bar));
        yield return new TestCaseData("7.2 l/min", new Quantity(7.2, UnitCatalog.LitrePerMinute));
    }

    [TestCaseSource(nameof(ParseCases))]
    public void ParseReadsValueAndUnit(string text, Quantity expected)
    {
        Assert.That(Quantity.Parse(text), Is.EqualTo(expected));
    }

    [Test]
    public void ParseReadsBackToStringOutput()
    {
        var quantity = new Quantity(3.25, UnitCatalog.Psi);

        Assert.That(Quantity.Parse(quantity.ToString()), Is.EqualTo(quantity));
    }

    [Test]
    public void ParsingUnknownSymbolThrows()
    {
        Assert.Throws<KeyNotFoundException>(() => Quantity.Parse("1.5 furlong"));
    }

    [TestCase("1.5bar")]
    [TestCase("abc bar")]
    [TestCase("1,5 bar")]
    public void ParsingTextThatCannotBeSplitOrReadThrows(string text)
    {
        Assert.Throws<FormatException>(() => Quantity.Parse(text));
    }

    // SetCulture changes process-global state for the test's duration.
    [Test]
    [SetCulture("de-DE")]
    public void ParseReadsTheCurrentCulturesDecimalSeparator()
    {
        var quantity = new Quantity(3.25, UnitCatalog.Psi);

        Assert.Multiple(() =>
        {
            Assert.That(Quantity.Parse(quantity.ToString()), Is.EqualTo(quantity));
            Assert.Throws<FormatException>(() => Quantity.Parse("1.5 bar"));
        });
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
