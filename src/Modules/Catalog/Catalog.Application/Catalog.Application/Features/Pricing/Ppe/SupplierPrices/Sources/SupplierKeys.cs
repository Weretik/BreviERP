using System.Text;
using System.Text.RegularExpressions;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

/// <summary>Normalizes supplier articles, links and names the way the Apps Script did, so files match products.</summary>
public static partial class SupplierKeys
{
    private static readonly Dictionary<char, char> LatinLookalikes = new()
    {
        ['A'] = 'А', ['B'] = 'В', ['C'] = 'С', ['E'] = 'Е', ['H'] = 'Н', ['K'] = 'К',
        ['M'] = 'М', ['O'] = 'О', ['P'] = 'Р', ['T'] = 'Т', ['X'] = 'Х', ['Y'] = 'У'
    };

    /// <summary>Upper case, Latin letters that look Cyrillic swapped, spaces, dashes, dots and underscores removed.</summary>
    public static string? Sku(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var builder = new StringBuilder();
        foreach (var character in value.Trim().ToUpperInvariant())
        {
            if (char.IsWhiteSpace(character) || character is '-' or '_' or '.') continue;
            builder.Append(LatinLookalikes.GetValueOrDefault(character, character));
        }

        return builder.Length == 0 ? null : builder.ToString();
    }

    /// <summary>Lower case, without scheme, www, query, trailing slash and the Insight language prefix (/ru/, /ua/).</summary>
    public static string? Url(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var url = value.Trim().ToLowerInvariant();
        url = SchemePattern().Replace(url, string.Empty);
        url = QueryPattern().Replace(url, string.Empty).TrimEnd('/');
        url = LanguagePattern().Replace(url, "$1/");
        return url.Length == 0 ? null : url;
    }

    /// <summary>Supplier names compared without case, spaces and punctuation, Latin lookalikes swapped.</summary>
    public static string Name(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value.Trim().ToUpperInvariant())
        {
            if (!char.IsLetterOrDigit(character)) continue;
            builder.Append(LatinLookalikes.GetValueOrDefault(character, character));
        }

        return builder.ToString();
    }

    [GeneratedRegex(@"^https?://(www\.)?")]
    private static partial Regex SchemePattern();

    [GeneratedRegex(@"[?#].*$")]
    private static partial Regex QueryPattern();

    [GeneratedRegex(@"^([^/]+)/(ru|ua|uk)/")]
    private static partial Regex LanguagePattern();
}
