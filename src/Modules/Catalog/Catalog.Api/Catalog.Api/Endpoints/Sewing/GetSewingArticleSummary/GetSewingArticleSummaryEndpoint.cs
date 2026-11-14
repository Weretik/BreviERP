using Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetSewingArticleSummary;

internal static class GetSewingArticleSummaryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSewingArticleSummary")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSewingArticleSummaryQuery(productId), cancellationToken)).ToHttpResult();
}
