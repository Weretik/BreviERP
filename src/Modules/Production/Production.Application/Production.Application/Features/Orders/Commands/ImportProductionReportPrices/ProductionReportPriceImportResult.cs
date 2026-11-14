namespace Production.Application.Features.Orders.Commands.ImportProductionReportPrices;

public sealed record ProductionReportPriceImportResult(
    bool DryRun, int Updated, int Unchanged, int Errors, IReadOnlyList<ProductionReportPriceImportLine> Lines);
