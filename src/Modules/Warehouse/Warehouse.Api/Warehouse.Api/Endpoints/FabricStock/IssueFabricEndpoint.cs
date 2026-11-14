using Warehouse.Application.Features.Stock.Commands.IssueFabric;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>«Разова витрата / без замовлення»; the balance may go below zero.</summary>
internal static class IssueFabricEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapPost("issues", HandleAsync).WithName("issueFabric").Produces<FabricStockOperationResponse>();

    private static async Task<IResult> HandleAsync(IssueFabricInput input, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new IssueFabricCommand(actor, input), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
