namespace Content.Domain.Redirects;

/// <summary>How an address of a redirect is written and checked (CNT-005 R-01, R-02).</summary>
public static class SiteRedirectRules
{
    public const int PathMaxLength = 500;
    public const int NoteMaxLength = 200;
    public const int MaxRedirects = 10_000;

    /// <summary>The locale prefixes of the new site; every target starts with one of them.</summary>
    public static readonly IReadOnlyList<string> SiteLocales = ["/uk", "/ru"];

    /// <summary>
    /// The path of an address as the site compares it: a full link loses its scheme and host, the query and fragment
    /// are dropped, percent-encoding is decoded, repeated and trailing slashes go, letters become lower case. Null
    /// when nothing is left, the result is the site root, it is longer than <see cref="PathMaxLength"/>, or it has
    /// spaces or control characters.
    /// </summary>
    public static string? Normalize(string? address)
    {
        if (string.IsNullOrWhiteSpace(address)) return null;
        var value = address.Trim();
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)) return null;
            value = uri.AbsolutePath;
        }

        var cut = value.IndexOfAny(['?', '#']);
        if (cut >= 0) value = value[..cut];
        try
        {
            value = Uri.UnescapeDataString(value);
        }
        catch (UriFormatException)
        {
            return null;
        }

        var segments = value.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0) return null;
        var path = "/" + string.Join('/', segments).ToLowerInvariant();
        if (path.Length > PathMaxLength || path.Any(c => char.IsWhiteSpace(c) || char.IsControl(c))) return null;
        return path;
    }

    /// <summary>A normalized path of the new site: <c>/uk</c>, <c>/ru</c> or a page under one of them.</summary>
    public static bool IsSitePath(string? path) =>
        path is not null && SiteLocales.Any(locale => path == locale || path.StartsWith(locale + "/", StringComparison.Ordinal));

    /// <summary>The same page of the new site in another language: <c>/uk/a</c> → <c>/ru/a</c>.</summary>
    public static string InLocale(string sitePath, string locale) =>
        IsSitePath(sitePath) ? "/" + locale + sitePath[3..] : sitePath;
}
