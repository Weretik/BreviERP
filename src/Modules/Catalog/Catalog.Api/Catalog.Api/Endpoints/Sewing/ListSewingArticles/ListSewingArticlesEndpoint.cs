using Catalog.Application.Features.Product.Queries.ListSewingArticles;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.ListSewingArticles;

internal static class ListSewingArticlesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("articles", HandleAsync).WithName("listSewingArticles")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListSewingArticlesQuery(productId), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Results.Ok(result.Value)
            : result.ToHttpResult();
    }
}
