using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Execution;
using CatalogImport.Planning;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;

namespace CatalogImport.Persistence;

public sealed class CatalogProductIdentityWriter(CatalogDbContext db, SourceMappingWriter mappings)
{
    public async Task<IReadOnlyDictionary<string, Product>> WriteAsync(CatalogImportPlan plan,
        ReferenceImportResult references, CatalogImportReport report, IReadOnlyList<WorkbookRow> models,
        IReadOnlyList<WorkbookRow> ppeRows, bool trial, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var categories = new CatalogCategoryResolver(db, mappings, report,
            await db.ProductCategories.ToListAsync(cancellationToken));
        var factory = new CatalogProductFactory(plan, report);
        var products = new Dictionary<string, Product>();
        var nextProduct = 0;
        foreach (var row in models.Concat(ppeRows))
        {
            var sewing = row.Sheet == "Товари";
            var product = factory.Create(row, ++nextProduct, now);
            product.AddCategory(categories.Resolve(row.Text(sewing ? 3 : 2)), now);
            var profile = CatalogProductProfileMapper.Create(row, product.Id, plan, references);
            if (sewing)
                factory.AddSewingIdentity(product, row, now);
            product.SetCatalogProfile(profile, now);
            products.Add(row.Text(0), product);
            db.Products.Add(product);
            mappings.Add(sewing ? "Model" : "Ppe", row.Text(0), "Product", product.Id.Value);
        }
        if (trial && products.Count != 1)
            throw new InvalidDataException("Trial fixture MOD-025 is absent.");
        await db.SaveChangesAsync(cancellationToken);
        return products;
    }
}

