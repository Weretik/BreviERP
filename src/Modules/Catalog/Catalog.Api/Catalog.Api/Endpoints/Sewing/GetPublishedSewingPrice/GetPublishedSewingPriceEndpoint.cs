using Catalog.Application.Abstractions.Pricing.Publications;
using Catalog.Application.Features.Pricing.Queries.GetPublishedSewingPrice;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetPublishedSewingPrice;

internal static class GetPublishedSewingPriceEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("published", HandleAsync).WithName("getPublishedSewingPrice")
            .WithSummary("Read the latest published price")
            .WithDescription("Read the latest published price. Historical published prices remain stored after input changes. Requires CatalogRead.")
            .RequireAuthorization("CatalogRead")
            .Produces<PublishedSewingPrice>();

    private static async Task<IResult> HandleAsync(int productId, long articleId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetPublishedSewingPriceQuery(productId, articleId), cancellationToken)).ToHttpResult();
}
