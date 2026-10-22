namespace LyxCsharp.Units;

/// <summary>Looks units up by symbol.</summary>
public static class UnitRegistry
{
    public static bool TryFind(string symbol, out IUnit unit)
    {
        var found = UnitCatalog.All.FirstOrDefault(
            u => string.Equals(u.Symbol, symbol, StringComparison.OrdinalIgnoreCase));
        unit = found!;
        return found is not null;
    }

    public static IUnit Find(string symbol) =>
        TryFind(symbol, out var unit)
            ? unit
            : throw new KeyNotFoundException($"Unknown unit '{symbol}'.");

    public static IEnumerable<IUnit> For(Dimension dimension) =>
        UnitCatalog.All.Where(u => u.Dimension == dimension);
}
