using Warehouse.Application.Features.Stock.Queries.GetFabricMovementHistory;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>«Історія правок» of a movement, newest first.</summary>
internal static class GetFabricMovementHistoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("movements/{movementId:int}/revisions", HandleAsync).WithName("getFabricMovementHistory").Produces<FabricMovementHistoryResponse>();

    private static async Task<IResult> HandleAsync(int movementId, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricMovementHistoryQuery(actor, movementId), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
