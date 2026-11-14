using Catalog.Application.Features.Pricing.Queries.GetPpeProductPrices;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.GetPpeProductPrices;

internal static class GetPpeProductPricesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPpeProductPrices")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync([FromQuery] int[] productIds, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeProductPricesQuery(productIds), cancellationToken)).ToHttpResult();
}
