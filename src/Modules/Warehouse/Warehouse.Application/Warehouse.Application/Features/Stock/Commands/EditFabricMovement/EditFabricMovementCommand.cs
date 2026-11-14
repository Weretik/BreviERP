using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.EditFabricMovement;

/// <summary>
/// Edits one movement: date, metres, supplier and document of a receipt, destination of an issue; a receipt may move to
/// another offer and colour, an issue to another pair the stock holds. The chains of both lines are restated.
/// </summary>
public sealed record EditFabricMovementCommand(WarehouseActor Actor, int MovementId, EditFabricMovementInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;
