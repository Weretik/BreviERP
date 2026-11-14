using Reference.Application.Features.SheetImport.Shared;

namespace Reference.Application.Features.GarmentPartOperation.Commands.ImportGarmentPartOperations;

/// <summary>
/// Preview or outcome of importing the sheet «Операції». <paramref name="Applied"/> is false for a preview and
/// whenever a row has an error: then nothing is saved.
/// </summary>
/// <param name="NewGroups">«Група деталей» names that do not exist yet and are created.</param>
/// <param name="Deleted">Operations missing from the sheet that are deleted.</param>
/// <param name="KeptInUse">Operations missing from the sheet that products or price history still use: they are
/// marked «Не використовується» instead of deleted.</param>
/// <param name="DeletedGroups">Groups left without operations that are deleted.</param>
public sealed record GarmentPartOperationImportResult(
    bool Applied,
    int Created,
    int Updated,
    int Unchanged,
    int Errors,
    IReadOnlyList<string> NewGroups,
    IReadOnlyList<string> Deleted,
    IReadOnlyList<string> KeptInUse,
    IReadOnlyList<string> DeletedGroups,
    IReadOnlyList<ReferenceImportRowResult> Rows);
