namespace Content.Application.Features.SiteGalleries.Shared;

/// <summary>
/// A picture for the public site. A file the site keeps has an address on the site itself (<c>/assets/…</c>);
/// a library photo has its CDN address.
/// </summary>
public sealed record PublicGalleryItem(
    string Url,
    string ThumbnailUrl,
    int? Width,
    int? Height,
    GalleryText Title,
    GalleryText Note,
    IReadOnlyList<string> Standards);
