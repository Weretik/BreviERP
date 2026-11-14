namespace Production.Application.Features.Orders.Shared;

/// <param name="SewingRate">Null for a manager.</param>
/// <param name="ReportUnitPrice">Null for anyone but Admin and Director.</param>
public sealed record ProductionItemResponse(
    int Id,
    int Position,
    string? ModelCode,
    string ModelName,
    string? FabricCode,
    string? FabricName,
    int Quantity,
    DateOnly DueDate,
    string MainColor,
    IReadOnlyList<string> AdditionalColors,
    string? SizesText,
    string? TechnicalChanges,
    string? Comment,
    IReadOnlyList<ItemApplicationResponse> Applications,
    decimal? PlannedFabricMeters,
    decimal? SewingRate,
    decimal? ReportUnitPrice,
    IReadOnlyList<string> Stages,
    string CurrentStage,
    int ShippedQuantity,
    IReadOnlyList<ProductionShipmentResponse> Shipments);
