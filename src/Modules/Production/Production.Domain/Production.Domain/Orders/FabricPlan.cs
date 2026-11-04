using Production.Domain.FabricNorms;

namespace Production.Domain.Orders;

/// <summary>
/// Planned fabric of an item: the model norm for its number of colours (main plus additional, at most three) times the
/// quantity. Null when the model has no norm for that number of colours.
/// </summary>
public static class FabricPlan
{
    public static decimal? Meters(FabricNormValues? norm, int colors, int quantity)
    {
        if (norm is null || quantity <= 0) return null;
        var perItem = Math.Clamp(colors, 1, 3) switch
        {
            1 => norm.OneColor,
            2 => norm.TwoColorsTotal,
            _ => norm.ThreeColorsTotal
        };
        return perItem is { } value ? decimal.Round(value * quantity, 2) : null;
    }
}
