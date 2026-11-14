namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

/// <summary>What Brevi gets.</summary>
public sealed record StockTransferTarget(
    int Lines,
    int Movements,
    int Revisions,
    int Issues,
    int IssueLines,
    int Plans,
    decimal Meters);
