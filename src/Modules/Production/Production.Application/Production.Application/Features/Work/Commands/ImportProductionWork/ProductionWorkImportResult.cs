namespace Production.Application.Features.Work.Commands.ImportProductionWork;

public sealed record ProductionWorkImportResult(
    bool DryRun, int Created, int Unchanged, int Errors, IReadOnlyList<ProductionWorkImportLine> Lines);
