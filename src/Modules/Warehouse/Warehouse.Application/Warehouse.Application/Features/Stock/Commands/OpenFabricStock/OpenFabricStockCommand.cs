using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Operations;

namespace Warehouse.Application.Features.Stock.Commands.OpenFabricStock;

/// <summary>«Початковий залишок»: once per fabric and colour (any code of its group), zero allowed.</summary>
public sealed record OpenFabricStockCommand(WarehouseActor Actor, OpenFabricStockInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;
