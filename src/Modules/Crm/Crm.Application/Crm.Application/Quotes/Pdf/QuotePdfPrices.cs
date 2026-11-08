namespace Crm.Application.Quotes.Pdf;

/// <summary>
/// The price tiles of one variant (R-30, R-35): only the quantities the client sees, in order and without gaps, so
/// hidden prices leave no empty place. When the variant costs the same in neighbouring quantities they become one
/// tile: «Ціна за 1 шт.», «до 40 шт.» or «від 10 шт.».
/// </summary>
public static class QuotePdfPrices
{
    private static readonly string[] Labels = ["до 9 шт.", "10–40 шт.", "від 41 шт."];

    public static IReadOnlyList<QuotePdfPriceTile> Tiles(decimal? retail, decimal? wholesale, decimal? large)
    {
        decimal?[] prices = [retail, wholesale, large];
        var shown = Enumerable.Range(0, Labels.Length).Where(tier => prices[tier] is not null).ToArray();
        if (shown.Length == 0) return [];

        var neighbours = shown[^1] - shown[0] == shown.Length - 1;
        var same = shown.All(tier => prices[tier] == prices[shown[0]]);
        if (shown.Length > 1 && neighbours && same)
            return [new QuotePdfPriceTile(RangeLabel(shown[0], shown[^1]), prices[shown[0]]!.Value)];

        return shown.Select(tier => new QuotePdfPriceTile(Labels[tier], prices[tier]!.Value)).ToArray();
    }

    private static string RangeLabel(int first, int last) => (first, last) switch
    {
        (0, 2) => "Ціна за 1 шт.",
        (0, 1) => "до 40 шт.",
        _ => "від 10 шт.",
    };
}
