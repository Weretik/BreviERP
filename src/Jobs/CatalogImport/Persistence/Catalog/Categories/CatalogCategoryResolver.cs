using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Execution;

namespace CatalogImport.Persistence;

public sealed class CatalogCategoryResolver(
    CatalogDbContext db, SourceMappingWriter mappings, CatalogImportReport report, List<ProductCategory> categories)
{
    private int _nextId = categories.Select(x => x.Id.Value).DefaultIfEmpty().Max();

    public ProductCategoryId Resolve(string name)
    {
        var matches = categories.Where(x => x.Name == name).ToArray();
        if (matches.Length > 1)
            throw new InvalidDataException($"Ambiguous category: {name}.");
        if (matches.Length == 1)
            return matches[0].Id;
        var category = ProductCategory.Create(ProductCategoryId.Create(++_nextId), name, name, $"import-category-{_nextId}");
        categories.Add(category);
        db.ProductCategories.Add(category);
        report.ReferenceChanges.Add($"Category created: {name} -> {_nextId}.");
        mappings.Add("Category", name, "ProductCategory", _nextId);
        return category.Id;
    }
}

