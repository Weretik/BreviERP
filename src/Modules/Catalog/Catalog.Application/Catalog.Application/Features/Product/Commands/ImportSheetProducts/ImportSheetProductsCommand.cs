
namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>
/// One-time import of all products from the Calculator sheet (CAT-010). Matched products keep their ID; the others
/// are created. With <paramref name="DeleteMissing"/>, products that are not in the sheet are deleted.
/// </summary>
/// <param name="DryRun">Only compute the report; nothing is saved.</param>
public sealed record ImportSheetProductsCommand(
    IReadOnlyList<SheetSewingRow> Sewing,
    IReadOnlyList<SheetPpeRow> Ppe,
    bool DryRun,
    bool DeleteMissing,
    string Actor) : ICommand<Result<SheetImportReport>>;
