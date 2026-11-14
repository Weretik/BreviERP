namespace Content.Application.Features.SiteGalleries.Shared;

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
