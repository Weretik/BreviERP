using CatalogImport.Execution;
using CatalogImport.Planning;
using Microsoft.EntityFrameworkCore;
using Reference.Infrastructure.DataBase;

namespace CatalogImport.Persistence;

public sealed class ReferenceCatalogWriter(ReferenceDbContext db, SourceMappingWriter mappings)
{
    public async Task<ReferenceImportResult> WriteAsync(CatalogImportPlan plan, CatalogImportReport report,
        CancellationToken cancellationToken)
    {
        var suppliers = new SupplierCatalogImporter(db, mappings, report, await db.Suppliers.ToListAsync(cancellationToken));
        suppliers.Import(plan.Snapshot);
        var result = new ReferenceImportResult
        {
            Fabrics = await new FabricCatalogImporter(db, mappings, suppliers, report).ImportAsync(plan.Snapshot, cancellationToken),
            Accessories = await new AccessoryCatalogImporter(db, mappings, suppliers, report).ImportAsync(plan.Snapshot, cancellationToken),
            Operations = await new OperationCatalogImporter(db, mappings, report).ImportAsync(plan, cancellationToken)
        };
        foreach (var row in plan.Ppe)
            suppliers.Resolve(row.Text(5));
        foreach (var (name, id) in suppliers.Resolved)
            result.Suppliers.Add(name, id);
        await db.SaveChangesAsync(cancellationToken);
        report.References = new Dictionary<string, int>
        {
            ["suppliers"] = result.Suppliers.Count,
            ["fabrics"] = result.Fabrics.Count,
            ["accessories"] = result.Accessories.Count,
            ["operations"] = result.Operations.Count
        };
        return result;
    }
}

