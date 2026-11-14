namespace Content.Application.Features.ProductPages.Commands.ImportOldSiteProductPages;

/// <summary>Compares links of the old site whatever the host, letter case or closing slash.</summary>
public static class OldSiteLinks
{
    /// <summary>The path of a link in lower case without the host, query and closing slash; empty for no link.</summary>
    public static string Path(string? url)
    {
        var text = (url ?? string.Empty).Trim().ToLowerInvariant();
        var scheme = text.IndexOf("://", StringComparison.Ordinal);
        if (scheme >= 0)
        {
            var slash = text.IndexOf('/', scheme + 3);
            text = slash < 0 ? string.Empty : text[slash..];
        }

        var end = text.IndexOfAny(['?', '#']);
        if (end >= 0) text = text[..end];
        return text.Trim('/');
    }

    /// <summary>The last part of the path: the product address on the old site.</summary>
    public static string LastPart(string? url)
    {
        var path = Path(url);
        return path[(path.LastIndexOf('/') + 1)..];
    }
}
