using Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;

namespace Catalog.Api.Endpoints.Sewing;

internal static class GetPublishedSewingProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublishedSewingProducts")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromQuery] int[] productIds, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublishedSewingProductsQuery(productIds), cancellationToken)).ToHttpResult();
}
