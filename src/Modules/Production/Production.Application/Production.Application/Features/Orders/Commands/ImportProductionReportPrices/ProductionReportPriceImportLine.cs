namespace Production.Application.Features.Orders.Commands.ImportProductionReportPrices;

/// <param name="Prices">Items that take a price; an item that already has one keeps it.</param>
/// <param name="Action">updated, unchanged or error.</param>
public sealed record ProductionReportPriceImportLine(
    int? Number, string LegacyId, int Prices, string Action, string? Error);
