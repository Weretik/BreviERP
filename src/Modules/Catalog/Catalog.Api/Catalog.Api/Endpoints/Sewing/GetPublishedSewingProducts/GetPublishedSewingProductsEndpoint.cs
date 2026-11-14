using Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetPublishedSewingProducts;

internal static class GetPublishedSewingProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublishedSewingProducts")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromQuery] int[] productIds, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublishedSewingProductsQuery(productIds), cancellationToken)).ToHttpResult();
}
