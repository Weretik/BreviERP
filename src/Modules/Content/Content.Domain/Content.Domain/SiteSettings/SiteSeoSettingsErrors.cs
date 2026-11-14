using BuildingBlocks.Domain.Errors;

namespace Content.Domain.SiteSettings;

/// <summary>Business rule errors of the site SEO settings.</summary>
public static class SiteSeoSettingsErrors
{
    public static Error InvalidAnalyticsId() =>
        Error.Validation("content.seo_invalid_ga4", "Ідентифікатор Google Analytics має вигляд G-XXXXXXX.", "GoogleAnalyticsId");

    public static Error InvalidTagManagerId() =>
        Error.Validation("content.seo_invalid_gtm", "Ідентифікатор Google Tag Manager має вигляд GTM-XXXXXX.", "GoogleTagManagerId");

    public static Error InvalidVerification(string field) =>
        Error.Validation("content.seo_invalid_verification",
            "Код підтвердження — латинські літери, цифри, - і _, до 100 символів.", field);
}
