using Warehouse.Application.Features.Stock.Queries.GetProductionItemFabric;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>The fabric card of an item: plan by part, issued, reserved and the issues with plan and fact.</summary>
internal static class GetFabricIssueItemEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("issue-items/{orderId:int}/{itemId:int}", HandleAsync).WithName("getFabricIssueItem").Produces<ProductionItemFabricResponse>();

    private static async Task<IResult> HandleAsync(int orderId, int itemId, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetProductionItemFabricQuery(actor, orderId, itemId), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
