namespace Warehouse.Application.Features.Stock.Commands.TransferStock.V133.Stock;

/// <summary><c>fabric_stock</c>: a stock line of a fabric (FAB- code) in a colour.</summary>
public sealed record V133StockRow(
    string Id,
    string FabricId,
    string? FabricName,
    string Color,
    string? NormalizedColor,
    long HundredthsOfMeter,
    string? LastMovementId,
    string? UpdatedAt);
