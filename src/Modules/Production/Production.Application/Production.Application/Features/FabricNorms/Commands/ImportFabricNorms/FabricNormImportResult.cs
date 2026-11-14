namespace Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;

public sealed record FabricNormImportResult(
    bool DryRun, int Created, int Updated, int Unchanged, int Errors, IReadOnlyList<FabricNormImportLine> Lines);
