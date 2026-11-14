using Warehouse.Application.Features.Stock.Queries.GetProductionItemFabric;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Api.Endpoints.OrderItemFabric;

/// <summary>Tab «Тканина» of an item: plan by colour, issued and the issues.</summary>
internal static class GetProductionItemFabricEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}/items/{itemId:int}/fabric", HandleAsync).WithName("getProductionItemFabric").Produces<ProductionItemFabricResponse>();

    private static async Task<IResult> HandleAsync(int id, int itemId, ClaimsPrincipal user, [FromServices] ISender sender, CancellationToken cancellationToken) =>
        user.WarehouseActor() is { } actor
            ? (await sender.Send(new GetProductionItemFabricQuery(actor, id, itemId), cancellationToken)).ToHttpResult()
            : Results.Forbid();
}
