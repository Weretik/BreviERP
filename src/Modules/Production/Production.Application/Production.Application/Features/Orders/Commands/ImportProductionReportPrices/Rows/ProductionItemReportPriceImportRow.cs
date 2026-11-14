namespace Production.Application.Features.Orders.Commands.ImportProductionReportPrices.Rows;

/// <param name="Position">The item's place in the order, from 1, as in production-orders.json.</param>
/// <param name="UnitPrice">UAH per item from the КП.</param>
public sealed record ProductionItemReportPriceImportRow(int Position, decimal UnitPrice);
