using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Features.Product.Shared.Overview;

namespace Catalog.Application.Features.Product.Commands.ImportSheetProducts;

/// <summary>What the import would write. It may be applied only when <see cref="Problems"/> is empty.</summary>
/// <param name="Writes">New and changed products in sheet order; unchanged ones are left out.</param>
/// <param name="Deletes">Stored products that are not in the sheet.</param>
/// <param name="Today">The import day in Kyiv.</param>
public sealed record SheetImportPlan(
    IReadOnlyList<SheetPlannedProduct> Writes,
    IReadOnlyList<ProductEntity> Deletes,
    SheetImportCounts Sewing,
    SheetImportCounts Ppe,
    IReadOnlyList<SheetImportIssue> Problems,
    IReadOnlyList<SheetImportIssue> Warnings,
    DateOnly Today)
{

    public SheetImportReport ToReport(bool applied) => new(
        applied,
        Sewing,
        Ppe,
        Deletes.OrderBy(x => x.Type).ThenBy(x => x.Id.Value)
            .Select(x => new SheetImportDeletion(x.Id.Value, ProductOverviewTypes.Name(x.Type), x.Code, x.Name))
            .ToList(),
        Problems,
        Warnings);

    public static string Scope(ProductType type) => type == ProductType.Ppe ? "ppe" : "sewing";
}
