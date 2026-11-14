using BuildingBlocks.Api.Caching;

namespace Content.Api.Endpoints.PublicSiteGalleries;

/// <summary>
/// Pictures of «Наші роботи» and «Сертифікати» for the public site (CNT-011): read-only, without sign-in. Only what
/// those pages show anyway.
/// </summary>
internal static class PublicSiteGalleriesEndpoints
{
    /// <summary>How long browsers and the CDN may keep an answer, as before on the controller.</summary>
    public const int CacheSeconds = 300;

    public static IEndpointRouteBuilder MapPublicSiteGalleriesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/galleries").AllowAnonymous()
            .WithPublicResponseCache(CacheSeconds)
            .WithTags("PublicSiteGalleries");
        GetPublicSiteGalleriesEndpoint.Map(group);
        return app;
    }
}
