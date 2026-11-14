using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Shared;

namespace Production.Api.Endpoints.Orders;

internal static class GetProductionOrdersEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("", HandleAsync).WithName("getProductionOrders");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, string? search = null,
        string? status = null, string? manager = null, DateOnly? dueTo = null, string? stage = null, int page = 1,
        int pageSize = 50, long? customerId = null, long? quoteId = null, string? client = null, DateOnly? from = null,
        DateOnly? to = null, string? model = null, string? fabric = null, string? color = null, string? season = null,
        string? sort = null, int? managerUserId = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(
            new GetProductionOrdersQuery(search, status, manager, dueTo, page, pageSize, stage, customerId, quoteId,
                new ProductionOrderFilters(client, from, to, model, fabric, color, season), sort, managerUserId),
            cancellationToken)).ToHttpResult();
}
