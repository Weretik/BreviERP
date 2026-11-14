namespace Production.Application.Features.Orders.Commands.ImportProductionReportPrices;

/// <summary>The КП prices of an order moved from the old «Цех», as in production-report-prices.json.</summary>
/// <param name="LegacyId">The order's id in the old app, as in production-orders.json.</param>
public sealed record ProductionReportPriceImportRow(string LegacyId, IReadOnlyList<ProductionItemReportPriceImportRow> Items);
