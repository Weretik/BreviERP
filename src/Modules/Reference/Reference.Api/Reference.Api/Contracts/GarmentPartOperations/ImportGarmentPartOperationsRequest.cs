using Reference.Application.Features.GarmentPartOperation.Import.DTOs;

namespace Reference.Api.Contracts.GarmentPartOperations;

/// <summary>Rows of the Calculator sheet «Операції»; dryRun returns the preview without saving.</summary>
public sealed record ImportGarmentPartOperationsRequest(
    IReadOnlyList<GarmentPartOperationImportRow> Rows,
    bool DryRun = true,
    bool DeleteMissing = false);
