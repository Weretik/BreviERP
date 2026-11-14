using Warehouse.Application.Abstractions;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetProductionItemFabric;

/// <summary>Tab «Тканина» of an item; everyone who sees the order sees it.</summary>
public sealed record GetProductionItemFabricQuery(WarehouseActor Actor, int OrderId, int ItemId)
    : IQuery<Result<ProductionItemFabricResponse>>;
