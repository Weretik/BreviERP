using Warehouse.Application.Features.Stock.Queries.GetFabricStock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Queries.GetFabricStock.Results;

namespace Warehouse.Api.Endpoints.FabricStock.GetFabricStock;

/// <summary>«Залишки»: fabrics with every colour the stock holds, alias groups summed.</summary>
internal static class GetFabricStockEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getFabricStock").Produces<FabricStockListResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricStockQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
