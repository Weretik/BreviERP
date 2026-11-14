namespace Content.Application.Features.SiteGalleries.Shared;

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
