namespace Crm.Application.Quotes.Pdf;

/// <summary>
/// The price columns of a КП item (R-30). A quantity no variant shows has no column and no label. When each variant
/// costs the same in neighbouring quantities, they become one column: «Ціна за 1 шт.», «до 40 шт.» or «від 10 шт.».
/// </summary>
public static class QuotePdfPrices
{
    private static readonly string[] Labels = ["до 9 шт.", "10–40 шт.", "від 41 шт."];

    public static IReadOnlyList<QuotePdfPriceColumn> Columns(IReadOnlyList<QuotePdfOption> options)
    {
        var rows = options.Select(x => new[] { x.Retail, x.Wholesale, x.Large }).ToArray();
        var shown = Enumerable.Range(0, Labels.Length).Where(tier => rows.Any(row => row[tier] is not null)).ToArray();
        if (shown.Length == 0) return [];

        var neighbours = shown[^1] - shown[0] == shown.Length - 1;
        var sameEverywhere = rows.All(row => shown.All(tier => row[tier] is not null && row[tier] == row[shown[0]]));
        if (shown.Length > 1 && neighbours && sameEverywhere)
            return [new QuotePdfPriceColumn(RangeLabel(shown[0], shown[^1]), rows.Select(row => row[shown[0]]).ToArray())];

        return shown.Select(tier => new QuotePdfPriceColumn(Labels[tier], rows.Select(row => row[tier]).ToArray()))
            .ToArray();
    }

    private static string RangeLabel(int first, int last) => (first, last) switch
    {
        (0, 2) => "Ціна за 1 шт.",
        (0, 1) => "до 40 шт.",
        _ => "від 10 шт.",
    };
}
