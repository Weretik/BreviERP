using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Operations;

namespace Warehouse.Application.Features.Stock.Commands.ReverseFabricMovement;

/// <summary>«Сторно» of an opening, receipt, correction or issue without an order; order issues are reversed on the order.</summary>
public sealed record ReverseFabricMovementCommand(WarehouseActor Actor, int MovementId, ReverseFabricMovementInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;
