using Content.Api.Access;

namespace Content.Api.Endpoints.SiteSettings;

/// <summary>«Контент → Налаштування SEO сайту» (CNT-006).</summary>
internal static class SiteSeoSettingsEndpoints
{
    public static IEndpointRouteBuilder MapSiteSeoSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/content/seo-settings").RequireAuthorization(ContentAccess.SeoSettingsPolicy)
            .WithTags("SiteSeoSettings");
        GetSiteSeoSettingsEndpoint.Map(group);
        SaveSiteSeoSettingsEndpoint.Map(group);
        return app;
    }
}
