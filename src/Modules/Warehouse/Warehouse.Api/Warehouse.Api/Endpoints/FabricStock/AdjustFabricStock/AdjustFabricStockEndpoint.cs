using Warehouse.Application.Features.Stock.Commands.AdjustFabricStock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock.AdjustFabricStock;

/// <summary>«Уточнити»: the counted total of a pair with a reason.</summary>
internal static class AdjustFabricStockEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("adjustments", HandleAsync).WithName("adjustFabricStock").Produces<FabricStockOperationResponse>();

    private static async Task<IResult> HandleAsync(AdjustFabricStockInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new AdjustFabricStockCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
