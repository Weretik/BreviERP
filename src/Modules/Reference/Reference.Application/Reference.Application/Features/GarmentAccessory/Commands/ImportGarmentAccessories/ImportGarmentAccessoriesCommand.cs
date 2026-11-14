using Reference.Application.Features.SheetImport.Shared;

namespace Reference.Application.Features.GarmentAccessory.Commands.ImportGarmentAccessories;

/// <param name="DryRun">Only compute the preview; nothing is saved.</param>
/// <param name="DeactivateMissing">Mark accessories that are not in the sheet as not used («Не використовується»).</param>
public sealed record ImportGarmentAccessoriesCommand(
    IReadOnlyList<GarmentAccessoryImportRow> Rows,
    bool DryRun,
    bool DeactivateMissing) : ICommand<Result<ReferenceImportResult>>;
