using Content.Api.Endpoints.PublicSitePages.GetPublicSitePages;
using BuildingBlocks.Api.Http;

namespace Content.Api.Endpoints.PublicSitePages;

/// <summary>
/// Search texts of the fixed pages of the public site (CNT-007): read-only, without sign-in. Only what every page
/// shows in its source anyway: the title and the description.
/// </summary>
internal static class PublicSitePagesEndpoints
{
    /// <summary>How long browsers and the CDN may keep an answer, as before on the controller.</summary>
    public const int CacheSeconds = 300;

    public static IEndpointRouteBuilder MapPublicSitePagesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/site-pages").AllowAnonymous()
            .WithPublicResponseCache(CacheSeconds)
            .WithTags("PublicSitePages");
        GetPublicSitePagesEndpoint.Map(group);
        return app;
    }
}
