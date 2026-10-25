using Catalog.Domain.Products.Entities;
using CatalogImport.Planning;

namespace CatalogImport.Persistence;

public sealed class CompositionSourceMappingWriter(SourceMappingWriter mappings)
{
    public void RecordArticles(Product product)
    {
        foreach (var article in product.Articles)
            mappings.Add("Article", article.SaleSku, "SewingArticle", article.Id.Value);
    }

    public void RecordLines(Product product, CatalogImportPlan plan)
    {
        var materialRows = plan.Materials.Where(line => line.Text(0) == product.ModelCode).ToArray();
        var operationRows = plan.Operations.Where(line => line.Text(1) == product.ModelCode).ToArray();
        foreach (var line in product.MaterialLines)
            mappings.Add("MaterialLine", materialRows[line.SortOrder].Key, "SewingMaterialLine", line.Id.Value);
        foreach (var line in product.OperationLines)
            mappings.Add("OperationLine", operationRows[line.SortOrder].Key, "SewingOperationLine", line.Id.Value);
    }
}

