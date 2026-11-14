namespace Content.Application.Features.SiteSettings.Shared;

/// <summary>
/// «Контент → Налаштування SEO сайту». <paramref name="Revision"/> is 0 before the first save.
/// <paramref name="DefaultOgImage"/> is null without a picture or when it was deleted from the library.
/// </summary>
public sealed record SiteSeoSettingsResponse(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    SitePicture? DefaultOgImage,
    int Revision,
    DateTimeOffset? UpdatedAtUtc,
    string? UpdatedBy);
