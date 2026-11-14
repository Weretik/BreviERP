namespace Warehouse.Application.Features.Stock.Commands.TransferStock.Transfer;

/// <summary>What Brevi gets.</summary>
public sealed record StockTransferTarget(
    int Lines,
    int Movements,
    int Revisions,
    int Issues,
    int IssueLines,
    int Plans,
    decimal Meters);
