namespace Warehouse.Application.Features.Stock.Commands.TransferStock.Transfer;

/// <summary>The Brevi stock before the transfer; <c>Own…</c> were entered in Brevi, not moved from the old «Цех».</summary>
public sealed record StockTransferCurrent(
    int Lines,
    int Movements,
    int Issues,
    int Plans,
    int OwnLines,
    int OwnMovements,
    int OwnIssues);
