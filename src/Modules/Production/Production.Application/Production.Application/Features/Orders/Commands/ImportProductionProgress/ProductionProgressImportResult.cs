using Production.Application.Features.Orders.Commands.ImportProductionProgress.Rows;

namespace Production.Application.Features.Orders.Commands.ImportProductionProgress;

public sealed record ProductionProgressImportResult(
    bool DryRun, int Updated, int Unchanged, int Errors, IReadOnlyList<ProductionProgressImportLine> Lines);
