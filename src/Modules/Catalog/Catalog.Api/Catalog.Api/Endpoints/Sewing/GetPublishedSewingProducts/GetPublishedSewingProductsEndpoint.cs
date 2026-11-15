using Catalog.Application.Abstractions.Pricing.Publications;
using Catalog.Application.Features.Pricing.Queries.GetPublishedSewingProducts;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetPublishedSewingProducts;

internal static class GetPublishedSewingProductsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPublishedSewingProducts")
            .WithSummary("Get published sewing products")
            .WithDescription("Requires CatalogRead. Returns active Sewing Products, their active Article count and the latest stored publication per active Article. Missing, inactive and non-Sewing Product IDs are omitted. Empty articles means no published price. No cost or actor data is exposed.")
            .RequireAuthorization("CatalogRead")
            .Produces<IReadOnlyList<SewingPublishedProductPrices>>();

    private static async Task<IResult> HandleAsync([FromQuery] int[] productIds, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublishedSewingProductsQuery(productIds), cancellationToken)).ToHttpResult();
}
