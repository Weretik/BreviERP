using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using CatalogImport.Execution;
using CatalogImport.Planning;
using CatalogImport.Source;

namespace CatalogImport.Persistence;

public sealed class CatalogProductFactory(CatalogImportPlan plan, CatalogImportReport report)
{
    private readonly Dictionary<string, int> _sourceNames = plan.Models.Concat(plan.Ppe).GroupBy(row => row.Text(1))
        .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

    public Product Create(WorkbookRow row, int id, DateTimeOffset now)
    {
        var sewing = row.Sheet == "Товари";
        var sourceKey = row.Text(0);
        var name = row.Text(1);
        var ruName = sewing ? name : plan.Snapshot.Rows("Прайс на сайт")
            .Single(price => price.Text(0) == sourceKey).Text(3);
        if (_sourceNames[name] > 1)
        {
            name += $" (SKU {sourceKey})";
            report.Warnings.Add($"{row.Key}: duplicate product name disambiguated by SKU.");
        }
        if (string.IsNullOrWhiteSpace(ruName))
            ruName = name;
        if (!sewing)
            ruName += $" (SKU {sourceKey})";
        return Product.Create(ProductId.Create(id), name, ruName,
            ProductSlug.Create(sewing ? $"model-{sourceKey.ToLowerInvariant()}" : $"ppe-{sourceKey.ToLowerInvariant()}"),
            sewing ? ProductType.Sewing : ProductType.Ppe, now);
    }

    public void AddSewingIdentity(Product product, WorkbookRow row, DateTimeOffset now)
    {
        product.SetModelCode(row.Text(0), now);
        foreach (var article in plan.Articles.Where(a => CatalogSourceIdentity.ModelCode(a) == row.Text(0)))
            product.AddArticle(SewingArticle.Create(ArticleId.Create(0), product.Id,
                article.Text(0), article.Flag(10), product.Articles.Count), now);
    }
}

