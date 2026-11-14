namespace Warehouse.Application.Features.Stock.Commands.TransferStock.Transfer;

/// <summary>Rows of the export and its control numbers.</summary>
public sealed record StockTransferSource(
    int Lines,
    int Pairs,
    int Movements,
    int Openings,
    int Receipts,
    int Issues,
    int Adjustments,
    int Voided,
    int Revisions,
    int Packages,
    int IssueLines,
    int Reservations,
    decimal Meters,
    int NegativePairs,
    int ZeroPairs);
