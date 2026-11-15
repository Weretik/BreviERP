using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierOffer;
using Catalog.Application.Features.Pricing.Queries.GetPpeSupplierOffer;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.GetPpeSupplierOffer;

internal static class GetPpeSupplierOfferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPpeSupplierOffer")
            .WithSummary("Get PPE supplier offer")
            .WithDescription("CatalogRead. The PPE product card (CAT-008): supplier fields from the sheet tab «ЗІЗ», Insight XML prices of the current offer version and the live three-tier calculation. Nothing is saved.")
            .RequireAuthorization("CatalogRead")
            .Produces<PpeSupplierOffer>();

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeSupplierOfferQuery(productId), cancellationToken)).ToHttpResult();
}
