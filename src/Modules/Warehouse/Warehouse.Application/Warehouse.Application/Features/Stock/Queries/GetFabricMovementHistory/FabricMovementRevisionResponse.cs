namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovementHistory;

/// <param name="Action">Edit, Void or Restore.</param>
/// <param name="IsUndone">Taken back by a later restore.</param>
public sealed record FabricMovementRevisionResponse(
    int Id,
    int Number,
    string Action,
    FabricMovementStateResponse Before,
    FabricMovementStateResponse After,
    string? Reason,
    string CreatedByName,
    DateTimeOffset CreatedAt,
    bool IsUndone);
