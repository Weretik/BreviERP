using Production.Application.Features.Orders.Queries.GetProductionModelSummary;
using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Filtering;
using BuildingBlocks.Api.Errors;

namespace Production.Api.Endpoints.Orders.GetProductionModelSummary;

/// <summary>«Зведення за моделями»: the same filters as the list, all pages, pieces by model.</summary>
internal static class GetProductionModelSummaryEndpoint
{
    public static void Map(RouteGroupBuilder group) =>
        group.MapGet("model-summary", HandleAsync).WithName("getProductionModelSummary")
            .WithSummary("«Зведення за моделями»: items of every order matching the list filters (all pages), grouped by model")
            .WithDescription("Takes the same filters as getProductionOrders without paging. Quantity is the ordered pieces. Season comes from the Catalog model; category is read from the model name.")
            .Produces<ProductionModelSummaryResponse>();

    private static async Task<IResult> HandleAsync([FromServices] ISender sender, string? search = null,
        string? status = null, string? manager = null, DateOnly? dueTo = null, string? stage = null,
        string? client = null, DateOnly? from = null, DateOnly? to = null, string? model = null, string? fabric = null,
        string? color = null, string? season = null, CancellationToken cancellationToken = default) =>
        (await sender.Send(new GetProductionModelSummaryQuery(
            new GetProductionOrdersQuery(search, status, manager, dueTo, Stage: stage,
                Filters: new ProductionOrderFilters(client, from, to, model, fabric, color, season))),
            cancellationToken)).ToHttpResult();
}
