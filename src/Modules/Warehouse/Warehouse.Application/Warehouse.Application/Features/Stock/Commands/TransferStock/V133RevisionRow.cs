namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

/// <summary><c>fabric_movement_revisions</c>: an edit or void of a movement with its state before and after (JSON).</summary>
public sealed record V133RevisionRow(
    string Id,
    string? OperationId,
    string MovementId,
    string BeforeJson,
    string AfterJson,
    string? ActorEmail,
    string At);
