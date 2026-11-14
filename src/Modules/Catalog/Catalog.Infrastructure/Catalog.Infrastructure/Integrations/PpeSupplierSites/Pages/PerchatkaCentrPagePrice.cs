using System.Text.RegularExpressions;

namespace Catalog.Infrastructure.Integrations.PpeSupplierSites.Pages;

/// <summary>
/// Finds the price on a Перчатка-центр product page like the script's perchatkaCentrWebsitePrice_: the patterns are
/// tried in the script's order and the first positive number wins.
/// </summary>
internal static class PerchatkaCentrPagePrice
{
    private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

    private static readonly Regex[] Patterns =
    [
        new(@"price-value=[""']([0-9\s.,]+)[""']", Options),
        new(@"(?:Ціна|Цена)[\s\S]{0,500}?([0-9\s]+[.,][0-9]{1,2})\s*(?:грн|₴)", Options),
        new(@"property=[""']product:price:amount[""'][^>]*content=[""']([0-9\s.,]+)[""']", Options),
        new(@"content=[""']([0-9\s.,]+)[""'][^>]*property=[""']product:price:amount[""']", Options),
        new(@"itemprop=[""']price[""'][^>]*content=[""']([0-9\s.,]+)[""']", Options),
        new(@"""price""\s*:\s*""([0-9\s.,]+)""", Options),
        new(@"data-price=[""']([0-9\s.,]+)[""']", Options),
        new(@"""price""\s*:\s*([0-9\s.,]+)", Options),
        new(@"itemprop=[""']price[""'][^>]*value=[""']([0-9\s.,]+)[""']", Options),
        new(@"([0-9\s]+[.,][0-9]{1,2})\s*(?:грн|₴)", Options),
        new(@"(?:Ціна|Цена)[^0-9]{0,100}([0-9\s]+[.,][0-9]{1,2})\s*(?:грн|₴)", Options),
    ];

    public static decimal? Find(string html)
    {
        foreach (var pattern in Patterns)
        {
            var match = pattern.Match(html);
            if (match.Success && SupplierPriceText.Strict(match.Groups[1].Value) is { } price) return price;
        }

        return null;
    }
}
