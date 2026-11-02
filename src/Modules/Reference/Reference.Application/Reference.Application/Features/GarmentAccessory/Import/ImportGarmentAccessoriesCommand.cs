using Reference.Application.Features.GarmentAccessory.Import.DTOs;
using Reference.Application.Features.Shared.SheetImport;

namespace Reference.Application.Features.GarmentAccessory.Import;

/// <param name="DryRun">Only compute the preview; nothing is saved.</param>
/// <param name="DeactivateMissing">Mark accessories that are not in the sheet as not used («Не використовується»).</param>
public sealed record ImportGarmentAccessoriesCommand(
    IReadOnlyList<GarmentAccessoryImportRow> Rows,
    bool DryRun,
    bool DeactivateMissing) : ICommand<Result<ReferenceImportResult>>;
