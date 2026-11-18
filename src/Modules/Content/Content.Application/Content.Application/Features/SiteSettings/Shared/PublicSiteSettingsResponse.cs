namespace Content.Application.Features.SiteSettings.Shared;

/// <summary>What the public site places on every page.</summary>
/// <remarks>Cached as is by <see cref="PublicSiteCache"/>, so it is immutable.</remarks>
[System.ComponentModel.ImmutableObject(true)]
public sealed record PublicSiteSettingsResponse(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    PublicSitePicture? DefaultOgImage);
