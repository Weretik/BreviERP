using Warehouse.Application.Features.Stock.Commands.RefreshItemFabricPlan;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>«Оновити план»: the saved plan of the item takes today's norm.</summary>
internal static class RefreshFabricItemPlanEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("issue-items/{orderId:int}/{itemId:int}/plan", HandleAsync).WithName("refreshFabricItemPlan").Produces<ProductionItemFabricResponse>().RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int orderId, int itemId, RefreshItemFabricPlanInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new RefreshItemFabricPlanCommand(actor, orderId, itemId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
