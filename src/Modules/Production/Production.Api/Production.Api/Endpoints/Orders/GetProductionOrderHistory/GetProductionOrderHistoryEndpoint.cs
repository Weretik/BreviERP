using Production.Application.Features.Orders.Queries.GetProductionOrderHistory;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.GetProductionOrderHistory;

internal static class GetProductionOrderHistoryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("{id:int}/history", HandleAsync).WithName("getProductionOrderHistory");

    private static async Task<IResult> HandleAsync(int id, [FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetProductionOrderHistoryQuery(id), cancellationToken)).ToHttpResult();
}
