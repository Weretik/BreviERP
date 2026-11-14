using Warehouse.Application.Features.Stock.Commands.RestoreFabricMovement;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock.RestoreFabricMovement;

/// <summary>«Скасувати останню правку»: the state before the last edit or void comes back.</summary>
internal static class RestoreFabricMovementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("movements/{movementId:int}/restore", HandleAsync).WithName("restoreFabricMovement").Produces<FabricStockOperationResponse>().RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int movementId, RestoreFabricMovementInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new RestoreFabricMovementCommand(actor, movementId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
