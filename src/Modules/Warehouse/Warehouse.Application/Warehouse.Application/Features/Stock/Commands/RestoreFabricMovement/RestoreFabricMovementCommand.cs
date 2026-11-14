using Warehouse.Application.Abstractions;
using Warehouse.Domain.Stock;
using Warehouse.Application.Features.Stock.Shared;

namespace Warehouse.Application.Features.Stock.Commands.RestoreFabricMovement;

/// <summary>
/// «Скасувати останню правку»: the state before the last edit or void comes back, only while the movement still is
/// what that revision left, so a restore never overwrites someone else's newer change.
/// </summary>
public sealed record RestoreFabricMovementCommand(WarehouseActor Actor, int MovementId,
    RestoreFabricMovementInput Input) : ICommand<Result<FabricStockOperationResponse>>;
