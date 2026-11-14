using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovementHistory;

/// <summary>«Історія правок» of a movement: every edit, void and restore with the values before and after.</summary>
public sealed record GetFabricMovementHistoryQuery(WarehouseActor Actor, int MovementId)
    : IQuery<Result<FabricMovementHistoryResponse>>;
