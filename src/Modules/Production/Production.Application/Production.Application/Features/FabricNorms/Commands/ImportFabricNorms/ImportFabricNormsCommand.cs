using Production.Application.Abstractions;
using Production.Domain.FabricNorms;

namespace Production.Application.Features.FabricNorms.Commands.ImportFabricNorms;

/// <summary>
/// Transfer of norms from the old «Цех»: dryRun returns the preview. Rows are saved only when none has an error, for
/// example a model code that is not in Catalog; norms not in the file stay.
/// </summary>
public sealed record ImportFabricNormsCommand(IReadOnlyList<FabricNormImportRow> Rows, bool DryRun, int? UserId)
    : ICommand<Result<FabricNormImportResult>>;
