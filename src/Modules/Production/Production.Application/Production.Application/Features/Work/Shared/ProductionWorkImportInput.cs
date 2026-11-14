using Production.Application.Features.Work.Commands.ImportProductionWork;
namespace Production.Application.Features.Work.Shared;

public sealed record ProductionWorkImportInput(IReadOnlyList<ProductionWorkImportRow> Rows);
