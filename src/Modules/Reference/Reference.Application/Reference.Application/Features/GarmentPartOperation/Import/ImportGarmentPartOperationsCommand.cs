using Reference.Application.Features.GarmentPartOperation.Import.DTOs;

namespace Reference.Application.Features.GarmentPartOperation.Import;

/// <param name="DryRun">Only compute the preview; nothing is saved.</param>
/// <param name="DeleteMissing">Delete operations and groups that are not in the sheet. An operation still used by
/// products or price history is marked «Не використовується» instead.</param>
public sealed record ImportGarmentPartOperationsCommand(
    IReadOnlyList<GarmentPartOperationImportRow> Rows,
    bool DryRun,
    bool DeleteMissing) : ICommand<Result<GarmentPartOperationImportResult>>;
