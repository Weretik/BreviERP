using BuildingBlocks.Api.Caching;
using Content.Api.Endpoints.PublicSiteSettings.GetPublicSiteSettings;

namespace Content.Api.Endpoints.PublicSiteSettings;

/// <summary>
/// Site-wide settings of the public site (CNT-006): read-only, without sign-in. Only what every visitor sees in the
/// page source anyway: analytics ids, verification codes and the default sharing picture.
/// </summary>
internal static class PublicSiteSettingsEndpoints
{
    /// <summary>How long browsers and the CDN may keep an answer, as before on the controller.</summary>
    public const int CacheSeconds = 300;

    public static IEndpointRouteBuilder MapPublicSiteSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/site-settings").AllowAnonymous()
            .WithPublicResponseCache(CacheSeconds)
            .WithTags("PublicSiteSettings");
        GetPublicSiteSettingsEndpoint.Map(group);
        return app;
    }
}
