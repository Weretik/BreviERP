namespace Warehouse.Application.Features.Stock.Commands.VoidFabricMovement;

/// <summary>«Скасувати»: the movement stays in the history with no effect, with a reason.</summary>
public sealed record VoidFabricMovementInput(Guid OperationId, int ExpectedRevision, string Reason);
