using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Operations;

namespace Warehouse.Application.Features.Stock.Commands.VoidFabricMovement;

/// <summary>«Скасувати»: the movement stays in the history with no effect on the balance; the original is in a revision.</summary>
public sealed record VoidFabricMovementCommand(WarehouseActor Actor, int MovementId, VoidFabricMovementInput Input)
    : ICommand<Result<FabricStockOperationResponse>>;
