using System.Text.RegularExpressions;

namespace Catalog.Infrastructure.Integrations.PpeSupplierSites;

/// <summary>
/// Finds the price on an АВ-Центр product page like the script's extractAvCenterPrice_: a JSON "price" first, then
/// «Ціна: … грн» in the visible text, then a number after a product-price or price-new/current class.
/// </summary>
internal static partial class AvCenterPagePrice
{
    public static decimal? Find(string html)
    {
        if (JsonPrice().Match(html) is { Success: true } json && SupplierPriceText.Loose(json.Groups[1].Value) is { } fromJson)
            return fromJson;

        var plain = Scripts().Replace(html, " ");
        plain = Styles().Replace(plain, " ");
        plain = Tags().Replace(plain, " ");
        plain = Nbsp().Replace(plain, " ");
        plain = Amp().Replace(plain, "&");
        plain = Spaces().Replace(plain, " ");
        if (VisiblePrice().Match(plain) is { Success: true } visible && SupplierPriceText.Loose(visible.Groups[1].Value) is { } fromText)
            return fromText;

        return SourcePrice().Match(html) is { Success: true } source ? SupplierPriceText.Loose(source.Groups[1].Value) : null;
    }

    private const RegexOptions Options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    [GeneratedRegex(@"[""']price[""']\s*:\s*[""']?([0-9][0-9\s.,]*)", Options)]
    private static partial Regex JsonPrice();

    [GeneratedRegex(@"<script[\s\S]*?</script>", Options)]
    private static partial Regex Scripts();

    [GeneratedRegex(@"<style[\s\S]*?</style>", Options)]
    private static partial Regex Styles();

    [GeneratedRegex(@"<[^>]+>")]
    private static partial Regex Tags();

    [GeneratedRegex(@"&nbsp;", Options)]
    private static partial Regex Nbsp();

    [GeneratedRegex(@"&amp;", Options)]
    private static partial Regex Amp();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();

    [GeneratedRegex(@"(?:ціна|цена)\s*:?\s*([0-9][0-9\s.,]{0,20})\s*(?:грн|₴)", Options)]
    private static partial Regex VisiblePrice();

    [GeneratedRegex(@"(?:product[-_]?price|price[-_]?(?:new|current))[^0-9]{0,180}([0-9][0-9\s .,]{0,20})", Options)]
    private static partial Regex SourcePrice();
}
