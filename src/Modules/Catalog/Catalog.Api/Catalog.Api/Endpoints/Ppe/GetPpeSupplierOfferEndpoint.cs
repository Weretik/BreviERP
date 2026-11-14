using Catalog.Application.Features.Pricing.Queries.GetPpeSupplierOffer;

namespace Catalog.Api.Endpoints.Ppe;

internal static class GetPpeSupplierOfferEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getPpeSupplierOffer")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPpeSupplierOfferQuery(productId), cancellationToken)).ToHttpResult();
}
