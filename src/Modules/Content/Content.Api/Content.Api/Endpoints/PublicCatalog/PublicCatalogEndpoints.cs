using BuildingBlocks.Api.Caching;

namespace Content.Api.Endpoints.PublicCatalog;

/// <summary>
/// The catalog of the public site (CNT-002 stage 4, CNT-003 stage 4): read-only, without sign-in. It returns only products marked
/// «Показувати на сайті» and nothing internal. Answers may be kept by the caller for five minutes.
/// </summary>
internal static class PublicCatalogEndpoints
{
    /// <summary>How long browsers and the CDN may keep an answer, as before on the controller.</summary>
    public const int CacheSeconds = 300;

    public static IEndpointRouteBuilder MapPublicCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/catalog").AllowAnonymous()
            .WithPublicResponseCache(CacheSeconds)
            .WithTags("PublicCatalog");
        GetPublicCatalogTreeEndpoint.Map(group);
        GetPublicCatalogProductsEndpoint.Map(group);
        GetPublicCatalogProductEndpoint.Map(group);
        GetPublicCatalogCategoriesEndpoint.Map(group);
        GetPublicCatalogCategoryEndpoint.Map(group);
        GetPublicCatalogCategorySlugsEndpoint.Map(group);
        GetPublicCatalogProductSlugsEndpoint.Map(group);
        return app;
    }
}
