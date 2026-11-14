namespace Warehouse.Application.Features.Stock.Queries.GetFabricMovementHistory;

/// <summary>The parts of a movement before or after a revision.</summary>
public sealed record FabricMovementStateResponse(
    string FabricCode,
    string Color,
    DateOnly OperationDate,
    decimal Delta,
    string? SupplierName,
    string? Document,
    string? Destination,
    bool IsVoided);
