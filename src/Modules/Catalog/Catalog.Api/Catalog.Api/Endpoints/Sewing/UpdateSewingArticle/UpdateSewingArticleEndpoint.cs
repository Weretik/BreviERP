using Catalog.Application.Features.Product.Shared.Sewing.Views;
using Catalog.Application.Features.Product.Commands.UpdateSewingArticle;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.UpdateSewingArticle;

/// <summary>Changes the sale SKU, activity and fabric of one fabric variant (CAT-015).</summary>
internal static class UpdateSewingArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("articles/{articleId:long}", HandleAsync).WithName("updateSewingArticle")
            .WithSummary("Change a fabric variant")
            .WithDescription("Requires access:products. Changes the sale SKU, activity and the variant's fabric with metres per product (CAT-015). A stale expectedVersion returns 409.")
            .RequireAuthorization("access:products")
            .Produces<SewingArticleView>();

    private static async Task<IResult> HandleAsync(int productId, long articleId, UpdateSewingArticleRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new UpdateSewingArticleCommand(productId, articleId, request),
            cancellationToken)).ToHttpResult();
}
