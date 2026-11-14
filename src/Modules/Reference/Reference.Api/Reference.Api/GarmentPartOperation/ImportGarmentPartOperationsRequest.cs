using Reference.Application.Features.GarmentPartOperation.Commands.ImportGarmentPartOperations;

namespace Reference.Api.GarmentPartOperation;

/// <summary>Rows of the Calculator sheet «Операції»; dryRun returns the preview without saving.</summary>
public sealed record ImportGarmentPartOperationsRequest(
    IReadOnlyList<GarmentPartOperationImportRow> Rows,
    bool DryRun = true,
    bool DeleteMissing = false);
