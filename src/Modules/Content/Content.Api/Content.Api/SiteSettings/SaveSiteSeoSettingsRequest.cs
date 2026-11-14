namespace Content.Api.SiteSettings;

public sealed record SaveSiteSeoSettingsRequest(
    int Revision,
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    int? DefaultOgPhotoId);
