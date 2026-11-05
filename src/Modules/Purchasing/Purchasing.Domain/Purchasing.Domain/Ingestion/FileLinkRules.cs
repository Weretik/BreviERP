using System.Text.RegularExpressions;

namespace Purchasing.Domain.Ingestion;

/// <summary>Finds original-document links in the FileLinks cell and the reasons a row needs a manual check.</summary>
public static partial class FileLinkRules
{
    [GeneratedRegex(@"https?://[^\s]+", RegexOptions.IgnoreCase)]
    private static partial Regex UrlPattern();

    [GeneratedRegex(@"[,;](?=https?://)", RegexOptions.IgnoreCase)]
    private static partial Regex GluedSeparator();

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex WholeNumber();

    /// <summary>Distinct exact links in their order; links glued with «,» or «;» are split, trailing «,;» removed.</summary>
    public static IReadOnlyList<string> Extract(string fileLinks)
    {
        var spaced = GluedSeparator().Replace(fileLinks, " ");
        var result = new List<string>();
        foreach (Match match in UrlPattern().Matches(spaced))
        {
            var url = match.Value.TrimEnd(',', ';');
            if (url.Length > 0 && !result.Contains(url, StringComparer.Ordinal)) result.Add(url);
        }

        return result;
    }

    /// <summary>A link the card may open: HTTPS, no user or password, host dropbox.com or www.dropbox.com.</summary>
    public static bool IsSafeDropbox(string url) =>
        Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && string.IsNullOrEmpty(uri.UserInfo)
        && (uri.Host.Equals("dropbox.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("www.dropbox.com", StringComparison.OrdinalIgnoreCase));

    /// <summary>Reasons for a manual check; the row stays in the register either way.</summary>
    public static IReadOnlyList<string> Issues(string fileLinks, string fileCount, IReadOnlyList<string> links)
    {
        var issues = new List<string>();
        var count = fileCount.Trim();
        if (!WholeNumber().IsMatch(count) || !int.TryParse(count, out var declared))
            issues.Add(count.Length == 0
                ? "FileCount порожній"
                : $"FileCount «{count}» не є цілим числом");
        else if (declared != links.Count)
            issues.Add($"FileCount = {declared}, а посилань знайдено {links.Count}");

        foreach (var link in links.Where(x => !IsSafeDropbox(x)))
            issues.Add($"Посилання не з Dropbox або небезпечне: {link}");

        if (fileLinks.Trim().Length > 0 && links.Count == 0)
            issues.Add("У FileLinks немає розпізнаних посилань");
        if (links.Count == 0)
            issues.Add("Немає посилання на оригінал");
        return issues;
    }
}
