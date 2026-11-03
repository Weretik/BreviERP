using Reference.Application.Features.Shared.SheetImport;

namespace Reference.Application.Features.ProductSection.Import.DTOs;

/// <summary>
/// Preview or outcome of importing «Типи і розділи». <paramref name="Applied"/> is false for a preview and
/// whenever a row has an error: then nothing is saved.
/// </summary>
/// <param name="Deleted">Brevi sections that are not in the sheet and are deleted.</param>
public sealed record ProductSectionImportResult(
    bool Applied,
    int Created,
    int Updated,
    int Unchanged,
    int Errors,
    IReadOnlyList<string> Deleted,
    IReadOnlyList<ReferenceImportRowResult> Rows);
