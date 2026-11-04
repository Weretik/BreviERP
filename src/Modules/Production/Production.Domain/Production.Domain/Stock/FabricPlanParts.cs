using Production.Domain.FabricNorms;

namespace Production.Domain.Stock;

/// <summary>A planned colour of an item: fabric, colour as typed, key, metres for the whole quantity.</summary>
public sealed record FabricPlanPart(string FabricCode, string Color, string ColorKey, decimal Meters)
{
    public FabricPlanKey Key => new(FabricCode, ColorKey);
}

/// <summary>
/// Plan of an item by colour (PRD-D28): one colour takes «1 колір»; two take main and additional; three take main and
/// both additional; × quantity, 0.01 m. A missing part of the norm plans nothing for that colour; the same colour
/// twice adds up.
/// </summary>
public static class FabricPlanParts
{
    public static IReadOnlyList<FabricPlanPart> For(FabricNormValues? norm, string? fabricCode, string mainColor,
        IReadOnlyList<string> additionalColors, int quantity)
    {
        if (norm is null || string.IsNullOrWhiteSpace(fabricCode) || quantity <= 0) return [];
        var colors = new[] { mainColor }.Concat(additionalColors).Where(x => !string.IsNullOrWhiteSpace(x)).Take(3)
            .ToArray();
        decimal?[] perItem = colors.Length switch
        {
            0 => [],
            1 => [norm.OneColor],
            2 => [norm.TwoColorsMain, norm.TwoColorsAdditional],
            _ => [norm.ThreeColorsMain, norm.ThreeColorsAdditional1, norm.ThreeColorsAdditional2]
        };
        var code = fabricCode.Trim().ToUpperInvariant();
        return colors.Zip(perItem)
            .Where(x => x.Second is > 0)
            .GroupBy(x => StockColor.Key(x.First))
            .Select(g => new FabricPlanPart(code, StockColor.Clean(g.First().First), g.Key,
                decimal.Round(g.Sum(x => x.Second!.Value) * quantity, FabricStock.Decimals)))
            .ToList();
    }
}
