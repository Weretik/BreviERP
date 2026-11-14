using Warehouse.Application.Features.Stock.Commands.VoidFabricMovement;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock.VoidFabricMovement;

/// <summary>«Скасувати»: the movement stays in the history with no effect on the balance.</summary>
internal static class VoidFabricMovementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("movements/{movementId:int}/void", HandleAsync).WithName("voidFabricMovement").Produces<FabricStockOperationResponse>().RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int movementId, VoidFabricMovementInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new VoidFabricMovementCommand(actor, movementId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
