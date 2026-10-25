using Catalog.Infrastructure.DataBase;
using CatalogImport.Execution;
using CatalogImport.Planning;

namespace CatalogImport.Persistence;

public sealed class CatalogProductWriter(CatalogDbContext db, SourceMappingWriter mappings)
{
    public async Task WriteAsync(CatalogImportPlan plan, ReferenceImportResult references,
        CatalogImportReport report, bool trial, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var models = trial ? plan.Models.Where(row => row.Text(0) == "MOD-025").ToArray() : plan.Models;
        var ppeRows = trial ? [] : plan.Ppe;
        var products = await new CatalogProductIdentityWriter(db, mappings)
            .WriteAsync(plan, references, report, models, ppeRows, trial, now, cancellationToken);
        await new CatalogCompositionWriter(db, mappings)
            .WriteAsync(plan, models, products, references, now, cancellationToken);
        report.Products = products.Count;
        report.Articles = products.Values.Sum(product => product.Articles.Count);
        report.Materials = products.Values.Sum(product => product.MaterialLines.Count);
        report.Operations = products.Values.Sum(product => product.OperationLines.Count);
    }
}
