using Warehouse.Application.Features.Stock.Commands.ReceiveFabric;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Operations;

namespace Warehouse.Api.Endpoints.FabricStock.ReceiveFabric;

/// <summary>«Прихід».</summary>
internal static class ReceiveFabricEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("receipts", HandleAsync).WithName("receiveFabric").Produces<FabricStockOperationResponse>();

    private static async Task<IResult> HandleAsync(ReceiveFabricInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new ReceiveFabricCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
