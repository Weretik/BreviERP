using Content.Domain.SiteGalleries;

namespace Content.Application.SiteGalleries.Shared;

/// <summary>
/// Where the site keeps its own pictures until they move to the «Фото» library (CNT-011 R-05): the published site,
/// whose files the transfer downloads, and the library folder that receives them.
/// </summary>
public static class SiteGalleryFiles
{
    /// <summary>The site with the files under <c>/assets/</c>; the Admin shows them from here until the transfer.</summary>
    public const string SiteOrigin = "https://brevi-site.pnevman.workers.dev";

    /// <summary>Mark of a transferred file in the library, so a repeated transfer finds it instead of downloading again.</summary>
    public const string SourcePrefix = "site-assets:";

    /// <summary>How many pictures one transfer request moves; the Admin repeats it until none are left.</summary>
    public const int TransferBatch = 10;

    public static IReadOnlyList<string> Folder(string key) =>
        key == SiteGallery.Certificates ? ["Сайт", "Сертифікати"] : ["Сайт", "Наші роботи"];

    public static Uri Url(string path) => new(SiteOrigin + path);

    public static string SourceKey(string path) => SourcePrefix + path;
}
