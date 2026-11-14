using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricIssueItems;

/// <summary>
/// «Видача на завдання»: items of open orders found by order number, customer or model, whatever fabric they plan
/// (TZ §7.1, A14); 30 at most, newest orders first.
/// </summary>
public sealed record GetFabricIssueItemsQuery(WarehouseActor Actor, string? Search)
    : IQuery<Result<FabricIssueItemsResponse>>;
