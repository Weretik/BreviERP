using Catalog.Application.Features.Product.Queries.GetProductOverviewItem;

namespace Catalog.Api.Endpoints.Overview.GetProductOverviewItem;

/// <summary>One «Товари» row with its prices and fabric variants, for the product card (CAT-013).</summary>
internal static class GetProductOverviewItemEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}/overview", HandleAsync).WithName("getProductOverviewItem")
            .RequireAuthorization("CatalogRead");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetProductOverviewItemQuery(id), cancellationToken)).ToHttpResult();
}
