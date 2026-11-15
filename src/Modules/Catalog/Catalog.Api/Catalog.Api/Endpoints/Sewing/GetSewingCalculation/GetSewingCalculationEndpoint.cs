using Catalog.Application.Abstractions.Pricing.Calculations;
using Catalog.Application.Features.Pricing.Queries.GetSewingCalculation;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetSewingCalculation;

internal static class GetSewingCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("calculations/{calculationId:guid}", HandleAsync).WithName("getSewingCalculation")
            .WithSummary("Read an immutable saved result scoped to its Product and Article")
            .WithDescription("Read an immutable saved result scoped to its Product and Article. Requires CatalogRead.")
            .RequireAuthorization("CatalogRead")
            .Produces<SavedSewingCalculation>();

    private static async Task<IResult> HandleAsync(int productId, long articleId, Guid calculationId,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetSewingCalculationQuery(productId, articleId, calculationId),
            cancellationToken)).ToHttpResult();
}
