using Catalog.Application.Features.Product.Shared.Sewing;
using Catalog.Application.Features.Product.Queries.GetSewingComposition;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetSewingComposition;

internal static class GetSewingCompositionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("composition", HandleAsync).WithName("getSewingComposition")
            .WithSummary("Get shared and effective Article composition")
            .WithDescription("Requires CatalogRead. Returns model-wide lines and shared operations when articleId is omitted. When articleId is supplied, also returns that Article's material lines; the effective materials are model-wide plus Article-scoped, and every material line identifies its scope.")
            .RequireAuthorization("CatalogRead")
            .Produces<SewingComposition>();

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender, long? articleId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetSewingCompositionQuery(productId, articleId), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? Results.Ok(result.Value)
            : result.ToHttpResult();
    }
}
