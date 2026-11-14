using Catalog.Api.Endpoints.Sewing.CreateSewingArticle;
using Catalog.Api.Endpoints.Sewing.DeleteSewingArticle;
using Catalog.Api.Endpoints.Sewing.ListSewingArticles;
using Catalog.Api.Endpoints.Sewing.UpdateSewingArticle;

namespace Catalog.Api.Endpoints.Sewing;

internal static class SewingArticlesEndpoints
{
    public static IEndpointRouteBuilder MapSewingArticlesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/v1/products/{productId:int}")
            .WithTags("SewingArticles");
        ListSewingArticlesEndpoint.Map(group);
        CreateSewingArticleEndpoint.Map(group);
        UpdateSewingArticleEndpoint.Map(group);
        DeleteSewingArticleEndpoint.Map(group);
        return app;
    }
}
