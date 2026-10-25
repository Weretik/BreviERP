using CatalogImport.Source;
using CatalogImport.Execution;
using CatalogImport.Planning;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace CatalogImport.Persistence;

public sealed class OperationCatalogImporter(ReferenceDbContext db, SourceMappingWriter mappings, CatalogImportReport report)
{
    public async Task<Dictionary<string, int>> ImportAsync(CatalogImportPlan plan, CancellationToken cancellationToken)
    {
        var parts = await db.GarmentParts.ToListAsync(cancellationToken);
        var operations = await db.GarmentPartOperations.ToListAsync(cancellationToken);
        var nextPart = parts.Select(x => x.Id.Value).DefaultIfEmpty().Max();
        var nextOperation = operations.Select(x => x.Id.Value).DefaultIfEmpty().Max();
        var result = new Dictionary<string, int>(StringComparer.Ordinal);

        GarmentPart PartFor(string name)
        {
            var part = parts.SingleOrDefault(x => x.Name == name);
            if (part is not null)
                return part;
            part = GarmentPart.Create(GarmentPartId.From(++nextPart), name);
            parts.Add(part);
            db.GarmentParts.Add(part);
            return part;
        }

        void Import(string sourceKey, string name, string partName, decimal minutes)
        {
            var part = PartFor(partName);
            var entity = operations.SingleOrDefault(x => x.Name == name && x.GarmentPartId == part.Id);
            if (entity is null)
            {
                entity = GarmentPartOperation.Create(GarmentPartOperationId.From(++nextOperation), part.Id, name, minutes);
                operations.Add(entity);
                db.GarmentPartOperations.Add(entity);
                report.ReferenceChanges.Add($"Operation created: {sourceKey} -> {entity.Id.Value}.");
            }
            else if (entity.Min != minutes)
            {
                report.ReferenceChanges.Add($"Operation {sourceKey} ID {entity.Id.Value}: minutes {entity.Min} -> {minutes}.");
                entity.Update(part.Id, entity.Name, minutes);
            }
            result.Add(sourceKey, entity.Id.Value);
            mappings.Add("Operation", sourceKey, "GarmentPartOperation", entity.Id.Value);
        }

        foreach (var row in plan.Snapshot.Rows("Операції"))
            Import(row.Text(0), row.Text(4), row.Text(1).Length == 0 ? row.Text(2) : row.Text(1), row.Decimal(5) ?? 0m);
        foreach (var row in plan.Operations.Where(line => line.Text(5).Length == 0))
            Import(CatalogSourceIdentity.OperationKey(row), row.Text(11), "Ручні операції", 0m);
        return result;
    }
}

