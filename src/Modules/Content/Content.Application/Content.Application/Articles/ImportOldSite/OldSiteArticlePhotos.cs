namespace Content.Application.Articles.ImportOldSite;

/// <summary>
/// Addresses of old-site pictures: only https on the old site itself and only JPG, PNG or WebP, so the transfer
/// cannot be pointed at another server.
/// </summary>
public static class OldSiteArticlePhotos
{
    public const int UrlMaxLength = 500;
    public const string SourcePrefix = "old-site:";
    private const string ImageRoot = "/image/";
    private static readonly string[] Hosts = ["brevi.com.ua", "www.brevi.com.ua"];
    private static readonly string[] Extensions = [".jpg", ".jpeg", ".png", ".webp"];

    /// <summary>Folder of the «Фото» library that receives the pictures of articles.</summary>
    public static readonly IReadOnlyList<string> Folder = ["Старий сайт", "Статті"];

    public static bool IsValid(string? url) => Parse(url) is not null;

    public static Uri? Parse(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > UrlMaxLength) return null;
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri)) return null;
        if (uri.Scheme != Uri.UriSchemeHttps || !uri.IsDefaultPort || uri.UserInfo.Length > 0) return null;
        if (!Hosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase)) return null;
        return Extensions.Contains(Path.GetExtension(uri.AbsolutePath), StringComparer.OrdinalIgnoreCase) ? uri : null;
    }

    /// <summary>
    /// The mark of the picture in the library: the path below <c>/image/</c>, the same as the photo transfer of
    /// products uses, so a picture both transfers meet is downloaded once.
    /// </summary>
    public static string SourceKey(Uri url)
    {
        var path = Uri.UnescapeDataString(url.AbsolutePath);
        return SourcePrefix + (path.StartsWith(ImageRoot, StringComparison.OrdinalIgnoreCase)
            ? path[ImageRoot.Length..]
            : path.TrimStart('/'));
    }
}
