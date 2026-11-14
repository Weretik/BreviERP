using Catalog.Application.Features.Pricing.Queries.GetSewingCalculation;

namespace Catalog.Api.Endpoints.Sewing;

internal static class GetSewingCalculationEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("calculations/{calculationId:guid}", HandleAsync).WithName("getSewingCalculation")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int productId, long articleId, Guid calculationId,
        [FromServices] ISender sender, CancellationToken cancellationToken) =>
        (await sender.Send(new GetSewingCalculationQuery(productId, articleId, calculationId),
            cancellationToken)).ToHttpResult();
}
