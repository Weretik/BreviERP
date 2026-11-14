using BuildingBlocks.Api.Caching;

namespace Content.Api.Endpoints.PublicRedirects;

/// <summary>
/// Redirects of the public site (CNT-005 stage 1): read-only, without sign-in. Each old address comes with the final
/// page, so the site answers one 301. Answers may be kept by the caller for five minutes.
/// </summary>
internal static class PublicRedirectsEndpoints
{
    /// <summary>How long browsers and the CDN may keep an answer, as before on the controller.</summary>
    public const int CacheSeconds = 300;

    public static IEndpointRouteBuilder MapPublicRedirectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/redirects").AllowAnonymous()
            .WithPublicResponseCache(CacheSeconds)
            .WithTags("PublicRedirects");
        GetPublicRedirectsEndpoint.Map(group);
        return app;
    }
}
