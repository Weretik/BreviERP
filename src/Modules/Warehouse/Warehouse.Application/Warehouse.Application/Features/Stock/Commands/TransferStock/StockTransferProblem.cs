namespace Warehouse.Application.Features.Stock.Commands.TransferStock;

/// <summary>A row the transfer refuses (error) or treats in its own way (note).</summary>
public sealed record StockTransferProblem(string Table, string? RowId, string Message);
