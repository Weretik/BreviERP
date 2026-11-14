using Catalog.Domain.Products;
using CatalogImport.Planning;
using CatalogImport.Source;
using Catalog.Domain.Products.Ppe;
using Catalog.Domain.Products.ValueObjects;

namespace CatalogImport.Persistence;

public static class CatalogProductProfileMapper
{
    public static ProductCatalogProfile Create(
        WorkbookRow row, ProductId productId, CatalogImportPlan plan, ReferenceImportResult references)
    {
        var sewing = row.Sheet == "Товари";
        var profile = ProductCatalogProfile.Create(productId, row.Flag(sewing ? 5 : 8));
        profile.SetIdentity(sewing ? null : row.Text(0), sewing ? row.Text(4) : null,
            sewing ? null : row.Text(3),
            sewing ? plan.Snapshot.ModelWebsiteUrls.GetValueOrDefault(row.Text(0)) : row.Text(4));
        if (!sewing)
            profile.SetSupplierOffer(SupplierReferenceId.Create(references.Suppliers[row.Text(5)]),
                row.Text(14), row.Text(6), row.Decimal(7), row.Flag(10), row.Text(13), row.Text(15));
        return profile;
    }
}

