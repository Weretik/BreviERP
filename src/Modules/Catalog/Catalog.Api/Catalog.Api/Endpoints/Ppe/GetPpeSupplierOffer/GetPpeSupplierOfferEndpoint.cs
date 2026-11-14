using Catalog.Application.Features.Pricing.Queries.GetPpeSupplierOffer;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Ppe.GetPpeSupplierOffer;

internal static class GetPpeSupplierOfferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPpeSupplierOffer")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeSupplierOfferQuery(productId), cancellationToken)).ToHttpResult();
}
