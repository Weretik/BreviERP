namespace Catalog.Application.Features.Product.OldSitePhotoImport;

/// <summary>Old-site image paths: relative, without «..», mapped to a library folder below «Старий сайт».</summary>
public static class OldSitePhotoPaths
{
    public static bool IsSafe(string? path)
        => !string.IsNullOrWhiteSpace(path) && !path.StartsWith('/') && !path.Contains('\\')
           && path.Split('/').All(x => x.Length > 0 && x != "." && x != "..") && !path.Contains("://");

    public static string SourceKey(string path) => ImportOldSitePhotosCommand.SourcePrefix + path.Trim();

    /// <summary>«catalog/costum_leto/agier/a.png» → «Старий сайт / costum_leto / agier»; the «catalog» root is dropped.</summary>
    public static IReadOnlyList<string> Folder(string path)
    {
        var parts = path.Trim().Split('/');
        var folders = parts[..^1].SkipWhile((x, index) => index == 0 && x == "catalog").Take(6);
        return [ImportOldSitePhotosCommand.RootFolder, .. folders];
    }

    /// <summary>The download address; each path segment is escaped (old names hold spaces).</summary>
    public static Uri Url(string baseUrl, string path)
        => new(new Uri(baseUrl), string.Join('/', path.Trim().Split('/').Select(Uri.EscapeDataString)));
}
