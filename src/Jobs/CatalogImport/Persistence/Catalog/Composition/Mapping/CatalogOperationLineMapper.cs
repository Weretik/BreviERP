using Catalog.Domain.Products;
using CatalogImport.Source;
using Catalog.Domain.Products.Sewing.Composition;
using Catalog.Domain.Products.Sewing.ValueObjects;
using Catalog.Domain.Products.ValueObjects;

namespace CatalogImport.Persistence;

public static class CatalogOperationLineMapper
{
    public static SewingOperationLine Create(WorkbookRow row, int index, ProductId productId, ReferenceImportResult references) =>
        SewingOperationLine.Create(CompositionLineId.Create(0), productId, null,
            OperationReferenceId.Create(references.Operations[CatalogSourceIdentity.OperationKey(row)]),
            row.Decimal(7)!.Value, row.Decimal(8), index).Require($"Operation line {index + 1} of product {productId}");
}

