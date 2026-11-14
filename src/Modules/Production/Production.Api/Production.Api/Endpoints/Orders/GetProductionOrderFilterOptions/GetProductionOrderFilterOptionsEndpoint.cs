using Production.Application.Features.Orders.Queries.GetProductionOrderFilterOptions;

namespace Production.Api.Endpoints.Orders.GetProductionOrderFilterOptions;

/// <summary>What «Фільтри» offer: clients, models, fabrics, colours and seasons of the orders.</summary>
internal static class GetProductionOrderFilterOptionsEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("filter-options", HandleAsync).WithName("getProductionOrderFilterOptions");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender,
        CancellationToken cancellationToken) =>
        (await sender.Send(new GetProductionOrderFilterOptionsQuery(), cancellationToken)).ToHttpResult();
}
