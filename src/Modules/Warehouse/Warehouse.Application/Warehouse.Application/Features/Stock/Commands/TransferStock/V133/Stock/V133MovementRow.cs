namespace Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;

/// <summary><c>fabric_stock_movements</c>: kind opening, receipt, issue, adjustment or void (a voided movement).</summary>
public sealed record V133MovementRow(
    string Id,
    string StockId,
    string? FabricId,
    string? FabricName,
    string? Color,
    string Kind,
    long BeforeHundredths,
    long DeltaHundredths,
    long AfterHundredths,
    string? OrderId,
    string? ItemId,
    string? OrderNumber,
    string? ModelName,
    string? ClientName,
    long? ItemQuantity,
    string? Supplier,
    string? Reference,
    string? Note,
    string? ActorEmail,
    string At,
    string? OperationDate);
