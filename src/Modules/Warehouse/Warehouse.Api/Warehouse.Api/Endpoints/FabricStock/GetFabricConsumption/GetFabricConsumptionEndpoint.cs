using Warehouse.Application.Features.Stock.Queries.GetFabricConsumption;
using Warehouse.Application.Features.Stock.Shared;
using BuildingBlocks.Api.Errors;

namespace Warehouse.Api.Endpoints.FabricStock.GetFabricConsumption;

/// <summary>«Зведення витрати» by operation date, both days included.</summary>
internal static class GetFabricConsumptionEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("consumption", HandleAsync).WithName("getFabricConsumption").Produces<FabricConsumptionResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricConsumptionQuery(actor, from ?? default, to ?? default), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
