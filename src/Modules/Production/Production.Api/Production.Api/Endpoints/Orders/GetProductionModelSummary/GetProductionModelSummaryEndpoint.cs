using Production.Application.Features.Orders.Queries.GetProductionModelSummary;
using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Filtering;

namespace Production.Api.Endpoints.Orders.GetProductionModelSummary;

/// <summary>«Зведення за моделями»: the same filters as the list, all pages, pieces by model.</summary>
internal static class GetProductionModelSummaryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("model-summary", HandleAsync).WithName("getProductionModelSummary");

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, string? search = null,
        string? status = null, string? manager = null, DateOnly? dueTo = null, string? stage = null,
        string? client = null, DateOnly? from = null, DateOnly? to = null, string? model = null, string? fabric = null,
        string? color = null, string? season = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetProductionModelSummaryQuery(
            new GetProductionOrdersQuery(search, status, manager, dueTo, Stage: stage,
                Filters: new ProductionOrderFilters(client, from, to, model, fabric, color, season))),
            cancellationToken)).ToHttpResult();
}
