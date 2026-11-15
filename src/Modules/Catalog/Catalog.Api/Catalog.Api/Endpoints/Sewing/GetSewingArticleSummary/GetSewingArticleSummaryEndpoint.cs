using Catalog.Application.Features.Pricing.Queries.GetSewingArticleSummary;
using BuildingBlocks.Api.Errors;

namespace Catalog.Api.Endpoints.Sewing.GetSewingArticleSummary;

internal static class GetSewingArticleSummaryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getSewingArticleSummary")
            .WithSummary("Get sewing article summary")
            .WithDescription("Requires CatalogRead. The sewing card in one response (CAT-007, CAT-016). Live calculation of every fabric variant (Article) at server time; nothing is saved; blocked variants return their blockers and no prices. Also returns each variant's fabric with its current name and price per metre, its latest published prices, and the shared composition (materials and operations) with reference names, unit prices and norm minutes. At most 200 Articles.")
            .RequireAuthorization("CatalogRead")
            .Produces<SewingArticleSummary>();

    private static async Task<IResult> HandleAsync(int productId, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetSewingArticleSummaryQuery(productId), cancellationToken)).ToHttpResult();
}
