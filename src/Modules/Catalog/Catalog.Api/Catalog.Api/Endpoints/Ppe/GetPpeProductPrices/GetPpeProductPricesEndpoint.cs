using Catalog.Application.Features.Pricing.Shared;
using Catalog.Application.Features.Pricing.Queries.GetPpeProductPrices;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.GetPpeProductPrices;

internal static class GetPpeProductPricesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPpeProductPrices")
            .WithSummary("Get PPE product prices")
            .WithDescription("CatalogRead. Bounded current PPE calculations from stored input versions; no live Sheet/XML or publication. Missing/non-PPE Products are omitted. Draft inputs yield blockers. Unenrolled Products are identified explicitly.")
            .RequireAuthorization("CatalogRead")
            .Produces<IReadOnlyList<PpeProductPricing>>();

    private static async Task<IResult> HandleAsync([FromQuery] int[] productIds, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeProductPricesQuery(productIds), cancellationToken)).ToHttpResult();
}
