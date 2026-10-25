using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using CatalogImport.Source;

namespace CatalogImport.Persistence;

public static class CatalogMaterialLineMapper
{
    public static SewingMaterialLine Create(WorkbookRow row, int index, Product product,
        IReadOnlyDictionary<string, SewingArticle> articles, ReferenceImportResult references)
    {
        var isFabric = row.Text(6).StartsWith("FAB-", StringComparison.Ordinal);
        var isService = row.Text(9) == "послуга";
        return SewingMaterialLine.Create(CompositionLineId.Create(0), product.Id,
            row.Text(1).Length == 0 ? null : articles[row.Text(1)].Id,
            isFabric ? SewingMaterialKind.Fabric : isService ? SewingMaterialKind.OtherCost : SewingMaterialKind.Accessory,
            isFabric ? FabricReferenceId.Create(references.Fabrics[row.Text(6)]) : null,
            !isFabric && !isService ? AccessoryReferenceId.Create(references.Accessories[row.Text(6)]) : null,
            isService ? row.Text(6) : null, row.Decimal(8)!.Value,
            row.Text(9) == "м" ? SewingMaterialUnit.Metre : isService ? SewingMaterialUnit.Service : SewingMaterialUnit.Piece,
            row.Flag(12), index);
    }
}

