using Warehouse.Application.Features.Stock.Commands.EditFabricMovement;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>Edits an opening, receipt, correction or issue without an order; both lines are restated.</summary>
internal static class EditFabricMovementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPut("movements/{movementId:int}", HandleAsync).WithName("editFabricMovement").Produces<FabricStockOperationResponse>();

    private static async Task<IResult> HandleAsync(int movementId, EditFabricMovementInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new EditFabricMovementCommand(actor, movementId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
