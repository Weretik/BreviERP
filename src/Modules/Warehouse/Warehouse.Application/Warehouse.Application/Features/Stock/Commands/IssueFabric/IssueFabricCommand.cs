using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.IssueFabric;

/// <summary>
/// «Разова витрата / без замовлення»: the fabric really left the shelf, so the balance may go below zero; it covers no
/// order plan.
/// </summary>
public sealed record IssueFabricCommand(WarehouseActor Actor, IssueFabricInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;
