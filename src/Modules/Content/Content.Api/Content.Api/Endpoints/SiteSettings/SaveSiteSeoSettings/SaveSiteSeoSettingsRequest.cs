namespace Content.Api.Endpoints.SiteSettings.SaveSiteSeoSettings;

public sealed record SaveSiteSeoSettingsRequest(
    int Revision,
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    int? DefaultOgPhotoId);
