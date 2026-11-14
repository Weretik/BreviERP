using System.Globalization;
using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online;

/// <summary>
/// The six published Google Sheets price lists of 3C-Маркет from the Apps Script. The article is in the third column
/// (digits only, leading zeros dropped) and the public price is the last number in the row.
/// </summary>
internal static class ThreeCMarketPriceLists
{
    private const string Published =
        "https://docs.google.com/spreadsheets/d/e/2PACX-1vRR43IHmO9NeOfzZYEzuTEpto4UaKyf5uXbbP-AgGw-fOkoMLW56Boyw8yjo_FhP3jiQCjKfqyFNeIp/pub?gid={0}&single=true&output=csv";

    public static readonly IReadOnlyList<(string Title, string Url)> All =
    [
        ("Одяг ЗІЗ", Csv("1429382662")),
        ("Взуття", Csv("851964583")),
        ("Рукавички", Csv("1852390840")),
        ("Захист органів дихання і голови", Csv("2042948821")),
        ("Захист очей та обличчя", Csv("2088354326")),
        ("Діелектрика", Csv("1392371230")),
    ];

    /// <summary>Article → public price of one list; rows without a digit article or a price are skipped.</summary>
    public static Dictionary<string, decimal> Parse(string csv)
    {
        var prices = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var row in CsvRows.Parse(csv))
        {
            if (row.Count < 3 || Article(row[2]) is not { } article) continue;
            // The script scanned down to the article column itself; stopping before it keeps an article from
            // being taken as a price in a row without one.
            for (var column = row.Count - 1; column > 2; column--)
            {
                if (SupplierPriceText.Loose(row[column]) is not { } price) continue;
                prices[article] = price;
                break;
            }
        }

        return prices;
    }

    /// <summary>Joins the lists by article; an article listed with different prices is reported, not guessed.</summary>
    public static IReadOnlyList<SupplierFilePrice> Merge(IEnumerable<(string Title, Dictionary<string, decimal> Prices)> lists)
    {
        var found = new Dictionary<string, List<(string Title, decimal Price)>>(StringComparer.Ordinal);
        foreach (var (title, prices) in lists)
        foreach (var (article, price) in prices)
        {
            if (!found.TryGetValue(article, out var entries)) found[article] = entries = [];
            entries.Add((title, price));
        }

        return found.Select(pair =>
        {
            var distinct = pair.Value.Select(x => x.Price).Distinct().ToList();
            return distinct.Count == 1
                ? new SupplierFilePrice(pair.Key, null, null, distinct[0], null, null, null)
                : new SupplierFilePrice(pair.Key, null, null, 0m, null, null,
                    "Артикул є в кількох прайсах 3C з різними цінами: " + string.Join(", ",
                        pair.Value.Select(x => $"«{x.Title}» {x.Price.ToString("0.##", CultureInfo.InvariantCulture)}")) + ".");
        }).ToList();
    }

    /// <summary>The script's normalize3cArticle_: only an all-digit value counts, leading zeros dropped.</summary>
    public static string? Article(string? value)
    {
        var article = (value ?? string.Empty).Replace("﻿", string.Empty).Trim();
        if (article.Length == 0 || !article.All(char.IsAsciiDigit)) return null;
        var trimmed = article.TrimStart('0');
        return trimmed.Length == 0 ? "0" : trimmed;
    }

    private static string Csv(string gid) => string.Format(CultureInfo.InvariantCulture, Published, gid);
}
