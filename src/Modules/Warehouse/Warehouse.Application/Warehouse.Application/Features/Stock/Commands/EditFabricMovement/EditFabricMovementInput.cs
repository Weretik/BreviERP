namespace Warehouse.Application.Features.Stock.Commands.EditFabricMovement;

/// <summary>
/// An edit of one movement. <paramref name="Meters"/> is the amount: positive for a receipt and an issue, from zero
/// for an opening, the signed non-zero change for an adjustment. A receipt may move to another offer
/// (<paramref name="FabricId"/>) and colour; an issue to another pair the stock holds (<paramref name="FabricCode"/>
/// and <paramref name="Color"/>). Supplier and document belong to a receipt, destination to an issue.
/// </summary>
/// <param name="ExpectedRevision">The movement's revision shown on the form.</param>
public sealed record EditFabricMovementInput(
    Guid OperationId,
    int ExpectedRevision,
    DateOnly OperationDate,
    decimal Meters,
    int? FabricId,
    string? FabricCode,
    string? Color,
    string? SupplierName,
    string? Document,
    string? Destination,
    string? Reason);
