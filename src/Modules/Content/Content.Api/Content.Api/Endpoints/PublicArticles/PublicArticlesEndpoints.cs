using BuildingBlocks.Api.Caching;
using Content.Api.Endpoints.PublicArticles.GetPublicArticle;
using Content.Api.Endpoints.PublicArticles.GetPublicArticles;

namespace Content.Api.Endpoints.PublicArticles;

/// <summary>
/// Articles of the public site (CNT-004 stage 4): read-only, without sign-in. Only articles marked «Показувати на сайті»
/// come back, and nothing internal. Answers may be kept by the caller for five minutes.
/// </summary>
internal static class PublicArticlesEndpoints
{
    /// <summary>How long browsers and the CDN may keep an answer, as before on the controller.</summary>
    public const int CacheSeconds = 300;

    public static IEndpointRouteBuilder MapPublicArticlesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/public/articles").AllowAnonymous()
            .WithPublicResponseCache(CacheSeconds)
            .WithTags("PublicArticles");
        GetPublicArticlesEndpoint.Map(group);
        GetPublicArticleEndpoint.Map(group);
        return app;
    }
}
