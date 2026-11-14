namespace Content.Application.Features.SiteSettings.Shared;

/// <summary>What the public site places on every page.</summary>
public sealed record PublicSiteSettingsResponse(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    PublicSitePicture? DefaultOgImage);
