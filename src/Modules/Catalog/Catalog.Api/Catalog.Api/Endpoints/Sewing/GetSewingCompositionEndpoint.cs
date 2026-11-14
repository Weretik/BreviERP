using Catalog.Application.Features.Product.Queries.GetSewingComposition;

namespace Catalog.Api.Endpoints.Sewing;

internal static class GetSewingCompositionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("composition", HandleAsync).WithName("getSewingComposition")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender, long? articleId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetSewingCompositionQuery(productId, articleId), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Results.Ok(result.Value)
            : result.ToHttpResult();
    }
}
