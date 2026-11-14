using Catalog.Application.Features.Product.Commands.DeleteSewingArticle;

namespace Catalog.Api.Endpoints.Sewing.DeleteSewingArticle;

/// <summary>Removes a fabric variant that has no saved prices yet (CAT-015).</summary>
internal static class DeleteSewingArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapDelete("articles/{articleId:long}", HandleAsync).WithName("deleteSewingArticle")
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int productId, long articleId, [FromServices] ISender sender,
        long expectedVersion = default, CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new DeleteSewingArticleCommand(productId, articleId, expectedVersion),
            cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? Results.NoContent() : result.ToHttpResult();
    }
}
