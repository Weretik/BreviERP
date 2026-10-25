using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using CatalogImport.Source;

namespace CatalogImport.Persistence;

public static class CatalogOperationLineMapper
{
    public static SewingOperationLine Create(WorkbookRow row, int index, ProductId productId, ReferenceImportResult references) =>
        SewingOperationLine.Create(CompositionLineId.Create(0), productId, null,
            OperationReferenceId.Create(references.Operations[CatalogSourceIdentity.OperationKey(row)]),
            row.Decimal(7)!.Value, row.Decimal(8), index, true);
}

