namespace Warehouse.Application.Features.Stock.Commands.RestoreFabricMovement;

/// <summary>«Скасувати останню правку»: the state before the last edit or void comes back.</summary>
public sealed record RestoreFabricMovementInput(Guid OperationId, int ExpectedRevision);
