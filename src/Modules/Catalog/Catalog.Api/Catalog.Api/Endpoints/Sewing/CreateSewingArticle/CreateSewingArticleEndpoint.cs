using Catalog.Application.Features.Product.Commands.CreateSewingArticle;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.CreateSewingArticle;

internal static class CreateSewingArticleEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("articles", HandleAsync).WithName("createSewingArticle")
            .RequireAuthorization("access:products");

    private static async Task<IResult> HandleAsync(int productId, CreateSewingArticleRequest request,
        [FromServices] ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateSewingArticleCommand(productId, request), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok)
            return result.ToHttpResult();

        return Results.CreatedAtRoute("listSewingArticles", new { productId }, result.Value);
    }
}
