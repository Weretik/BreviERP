using Warehouse.Application.Features.Stock.Queries.GetFabricStockChoices;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.FabricStock;

/// <summary>Active offers of «Тканини», supplier names and colours for the forms.</summary>
internal static class GetFabricStockChoicesEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("choices", HandleAsync).WithName("getFabricStockChoices").Produces<FabricStockChoicesResponse>();

    private static async Task<IResult> HandleAsync(ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetFabricStockChoicesQuery(actor), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
