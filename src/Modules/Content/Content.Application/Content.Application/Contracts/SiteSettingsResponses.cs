namespace Content.Application.Contracts;

/// <summary>A picture of the «Фото» library as the site uses it.</summary>
public sealed record SitePicture(int PhotoId, string Url, int? Width, int? Height);

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

/// <summary>What the public site places on every page.</summary>
public sealed record PublicSiteSettingsResponse(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    PublicSitePicture? DefaultOgImage);

public sealed record PublicSitePicture(string Url, int? Width, int? Height);
