using LyxCsharp.Units;

namespace LyxCsharp.Units.Tests;

public class UnitRegistryTests
{
    [TestCase("bar")]
    [TestCase("BAR")]
    [TestCase("kPa")]
    [TestCase("l/min")]
    public void FindsKnownSymbols(string symbol)
    {
        Assert.That(UnitRegistry.TryFind(symbol, out var unit), Is.True);
        Assert.That(unit.Symbol, Is.EqualTo(symbol).IgnoreCase);
    }

    [Test]
    public void TryFindReturnsFalseForUnknownSymbol()
    {
        Assert.That(UnitRegistry.TryFind("furlong", out _), Is.False);
    }

    [Test]
    public void FindThrowsForUnknownSymbol()
    {
        Assert.Throws<KeyNotFoundException>(() => UnitRegistry.Find("furlong"));
    }

    [TestCase(Dimension.Pressure, 4)]
    [TestCase(Dimension.Length, 4)]
    [TestCase(Dimension.Flow, 3)]
    public void ListsUnitsPerDimension(Dimension dimension, int expectedCount)
    {
        Assert.That(UnitRegistry.For(dimension).Count(), Is.EqualTo(expectedCount));
    }

    [Test]
    public void SymbolsAreUnique()
    {
        var symbols = UnitCatalog.All.Select(u => u.Symbol).ToList();

        Assert.That(symbols, Is.Unique);
    }

    [Test]
    [Category("Slow")]
    public void EveryUnitRoundTripsAcrossManyValues()
    {
        foreach (var unit in UnitCatalog.All)
        {
            for (var i = 1; i <= 200_000; i++)
            {
                var value = i * 0.37;
                var back = unit.FromBase(unit.ToBase(value));
                Assert.That(back, Is.EqualTo(value).Within(1e-6));
            }
        }
    }
}
