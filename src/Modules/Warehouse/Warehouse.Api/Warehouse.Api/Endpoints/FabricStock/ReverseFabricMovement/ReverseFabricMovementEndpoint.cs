using Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Operations;
using BuildingBlocks.Api.Errors;

namespace Warehouse.Api.Endpoints.FabricStock.ReverseFabricMovement;

/// <summary>«Сторно» of an opening, receipt, correction or issue without an order.</summary>
internal static class ReverseFabricMovementEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("movements/{movementId:int}/reverse", HandleAsync).WithName("reverseFabricMovement").Produces<FabricStockOperationResponse>().RequireAccessAction(AccessActionAttribute.Edit);

    private static async Task<IResult> HandleAsync(int movementId, ReverseFabricMovementInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new ReverseFabricMovementCommand(actor, movementId, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
