using Catalog.Application.Features.Product.Shared.Sewing.Views;
using Catalog.Application.Features.Product.Commands.CreateSewingArticle;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.CreateSewingArticle;

internal static class CreateSewingArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("articles", HandleAsync).WithName("createSewingArticle")
            .WithSummary("Create a fabric variant of a Sewing Product")
            .WithDescription("Requires access:products. Creates the variant (Article) with its fabric and metres per product in one transaction (CAT-015). Sale SKU is globally unique, trimmed and compared using ordinal case-sensitive equality.")
            .RequireAuthorization("access:products")
            .Produces<SewingArticleView>(StatusCodes.Status201Created);

    private static async Task<IResult> HandleAsync(int productId, CreateSewingArticleRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateSewingArticleCommand(productId, request), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok)
            return result.ToHttpResult();

        return Results.CreatedAtRoute("listSewingArticles", new { productId }, result.Value);
    }
}
