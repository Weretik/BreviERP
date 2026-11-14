using Catalog.Application.Features.Pricing.Queries.GetPublishedSewingPrice;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetPublishedSewingPrice;

internal static class GetPublishedSewingPriceEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("published", HandleAsync).WithName("getPublishedSewingPrice")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int productId, long articleId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublishedSewingPriceQuery(productId, articleId), cancellationToken)).ToHttpResult();
}
