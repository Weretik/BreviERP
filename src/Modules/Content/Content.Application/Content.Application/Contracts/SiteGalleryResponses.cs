namespace Content.Application.Contracts;

/// <summary>A text in Ukrainian and Russian; an empty Russian text shows the Ukrainian one on the site.</summary>
public sealed record GalleryText(string? Uk, string? Ru);

/// <summary>
/// One picture as staff send it (CNT-011): a photo of the «Фото» library, or a file the site still keeps
/// (<paramref name="LegacyPath"/>, only one the gallery already has).
/// </summary>
public sealed record SiteGalleryItemInput(
    int? PhotoId,
    string? LegacyPath,
    GalleryText? Title,
    GalleryText? Note,
    IReadOnlyList<string>? Standards);

/// <summary>
/// One picture of «Наші роботи» or «Сертифікати». <paramref name="Url"/> and <paramref name="ThumbnailUrl"/> are full
/// addresses for the Admin; <paramref name="Missing"/> is true when the library no longer has the photo.
/// </summary>
public sealed record SiteGalleryItemResponse(
    int? PhotoId,
    string? LegacyPath,
    string? Url,
    string? ThumbnailUrl,
    int? Width,
    int? Height,
    bool Missing,
    GalleryText Title,
    GalleryText Note,
    IReadOnlyList<string> Standards);

/// <summary>
/// A page of «Контент → Наші роботи» or «Контент → Сертифікати». <paramref name="LegacyCount"/> counts the pictures that
/// are still files of the site and wait for «Перенести у бібліотеку фото».
/// </summary>
public sealed record SiteGalleryResponse(
    string Key,
    IReadOnlyList<SiteGalleryItemResponse> Items,
    int LegacyCount,
    int Revision,
    DateTimeOffset? UpdatedAtUtc,
    string? UpdatedBy);

/// <summary>One step of the transfer: how many pictures moved now, how many still wait, and why some failed.</summary>
public sealed record SiteGalleryTransferResponse(
    int Moved,
    int Left,
    IReadOnlyList<string> Problems,
    SiteGalleryResponse Gallery);

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

public sealed record PublicSiteGalleriesResponse(
    IReadOnlyList<PublicGalleryItem> Works,
    IReadOnlyList<PublicGalleryItem> Certificates);
