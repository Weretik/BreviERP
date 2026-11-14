using Catalog.Application.Features.Product.Commands.UpdateSewingArticle;

namespace Catalog.Api.Endpoints.Sewing.UpdateSewingArticle;

/// <summary>Changes the sale SKU, activity and fabric of one fabric variant (CAT-015).</summary>
internal static class UpdateSewingArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("articles/{articleId:long}", HandleAsync).WithName("updateSewingArticle")
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int productId, long articleId, UpdateSewingArticleRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateSewingArticleCommand(productId, articleId, request),
            cancellationToken)).ToHttpResult();
}
