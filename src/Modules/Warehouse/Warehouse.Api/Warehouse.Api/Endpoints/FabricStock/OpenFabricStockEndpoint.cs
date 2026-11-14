using Warehouse.Application.Features.Stock.Commands.OpenFabricStock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>«Початковий залишок» of a new fabric and colour; once per pair.</summary>
internal static class OpenFabricStockEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("openings", HandleAsync).WithName("openFabricStock").Produces<FabricStockOperationResponse>();

    private static async Task<IResult> HandleAsync(OpenFabricStockInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new OpenFabricStockCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
