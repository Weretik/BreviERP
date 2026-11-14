using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Orders.Queries.GetProductionOrderHistory;

/// <summary>«Історія змін», newest first.</summary>
public sealed record GetProductionOrderHistoryQuery(int Id) : IQuery<Result<IReadOnlyList<ProductionOrderChangeResponse>>>;
