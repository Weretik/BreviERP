using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Queries.GetProductionOrders;
using Production.Application.Features.Orders.Shared;

namespace Production.Application.Features.Orders.Queries.GetProductionModelSummary;

/// <summary>
/// «Зведення за моделями»: every order of the list filters (all pages), items grouped by model with the ordered
/// pieces. <paramref name="Filter"/> is the list query; its page is ignored.
/// </summary>
public sealed record GetProductionModelSummaryQuery(GetProductionOrdersQuery Filter)
    : IQuery<Result<ProductionModelSummaryResponse>>;
