using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.AdjustFabricStock;

/// <summary>
/// «Уточнити»: the counted total of a pair replaces its balance through one adjustment with a reason. It may not end
/// below min(0, current total); a real shortage is recorded as an issue instead.
/// </summary>
public sealed record AdjustFabricStockCommand(WarehouseActor Actor, AdjustFabricStockInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;
