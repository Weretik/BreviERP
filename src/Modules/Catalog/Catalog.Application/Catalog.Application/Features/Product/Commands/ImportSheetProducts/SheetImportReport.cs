namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>
/// Preview or outcome of the sheet product import. <paramref name="Applied"/> is false for a preview and whenever
/// <paramref name="Problems"/> is not empty: then nothing is saved. <paramref name="Warnings"/> never stop the import.
/// </summary>
/// <param name="ToDelete">Products that are not in the sheet and are deleted (only with deleteMissing).</param>
public sealed record SheetImportReport(
    bool Applied,
    SheetImportCounts Sewing,
    SheetImportCounts Ppe,
    IReadOnlyList<SheetImportDeletion> ToDelete,
    IReadOnlyList<SheetImportIssue> Problems,
    IReadOnlyList<SheetImportIssue> Warnings);
