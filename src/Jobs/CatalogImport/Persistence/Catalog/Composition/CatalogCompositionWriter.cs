using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DataBase;
using CatalogImport.Planning;
using CatalogImport.Source;

namespace CatalogImport.Persistence;

public sealed class CatalogCompositionWriter(CatalogDbContext db, SourceMappingWriter mappings)
{
    public async Task WriteAsync(CatalogImportPlan plan, IReadOnlyList<WorkbookRow> models,
        IReadOnlyDictionary<string, Product> products, ReferenceImportResult references,
        DateTimeOffset now, CancellationToken cancellationToken)
    {
        var sourceMappings = new CompositionSourceMappingWriter(mappings);
        foreach (var row in models)
        {
            var product = products[row.Text(0)];
            var articles = product.Articles.ToDictionary(article => article.SaleSku, StringComparer.Ordinal);
            sourceMappings.RecordArticles(product);
            var materials = plan.Materials.Where(line => line.Text(0) == row.Text(0))
                .Select((line, index) => CatalogMaterialLineMapper.Create(line, index, product, articles, references)).ToArray();
            var operations = plan.Operations.Where(line => line.Text(1) == row.Text(0))
                .Select((line, index) => CatalogOperationLineMapper.Create(line, index, product.Id, references)).ToArray();
            // Source review is not a Brevi user review; imported lines remain unreviewed.
            product.ReplaceComposition(materials, operations, now);
        }
        await db.SaveChangesAsync(cancellationToken);
        foreach (var row in models)
            sourceMappings.RecordLines(products[row.Text(0)], plan);
    }
}

