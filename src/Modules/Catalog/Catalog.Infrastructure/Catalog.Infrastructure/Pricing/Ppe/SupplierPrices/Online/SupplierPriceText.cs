using System.Globalization;

namespace Catalog.Infrastructure.Pricing.Ppe.SupplierPrices.Online;

/// <summary>Turns a price written on a supplier page or list into a number, as the Apps Script did.</summary>
internal static class SupplierPriceText
{
    /// <summary>
    /// The script's normalizePrice_: spaces removed, the first comma becomes a dot, other characters dropped;
    /// only a positive number counts.
    /// </summary>
    public static decimal? Loose(string? value)
    {
        if (value is null) return null;
        var text = new string(value.Where(character => !char.IsWhiteSpace(character)).ToArray());
        var comma = text.IndexOf(',');
        if (comma >= 0) text = string.Concat(text.AsSpan(0, comma), ".", text.AsSpan(comma + 1));
        text = new string(text.Where(character => char.IsAsciiDigit(character) || character == '.').ToArray());
        return Positive(text);
    }

    /// <summary>Перчатка-центр: spaces removed and the first comma becomes a dot; anything else makes it not a number.</summary>
    public static decimal? Strict(string? value)
    {
        if (value is null) return null;
        var text = new string(value.Where(character => !char.IsWhiteSpace(character)).ToArray());
        var comma = text.IndexOf(',');
        if (comma >= 0) text = string.Concat(text.AsSpan(0, comma), ".", text.AsSpan(comma + 1));
        return Positive(text);
    }

    private static decimal? Positive(string text)
        => decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price) && price > 0
            ? price
            : null;
}
