using BuildingBlocks.Domain.ValueObjects;
using CatalogImport.Execution;
using CatalogImport.Planning;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace CatalogImport.Persistence;

public sealed class FabricCatalogImporter(
    ReferenceDbContext db, SourceMappingWriter mappings, SupplierCatalogImporter suppliers, CatalogImportReport report)
{
    public async Task<Dictionary<string, int>> ImportAsync(WorkbookSnapshot snapshot, CancellationToken cancellationToken)
    {
        var fabrics = await db.Fabrics.ToListAsync(cancellationToken);
        var nextId = fabrics.Select(x => x.Id.Value).DefaultIfEmpty().Max();
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        var rows = CatalogSourceReferences.Fabrics(snapshot);
        foreach (var row in rows)
        {
            var supplierId = suppliers.Resolve(row.Text(2));
            var duplicateName = rows.Count(other => other.Text(1) == row.Text(1)) > 1;
            var name = duplicateName ? $"{row.Text(1)} [{row.Text(0)}]" : row.Text(1);
            if (duplicateName)
                report.Warnings.Add($"{row.Key}: duplicate fabric label disambiguated by source code.");
            var entity = fabrics.SingleOrDefault(x => x.Name == name);
            var cost = row.Decimal(5)!.Value;
            if (entity is null)
            {
                entity = Fabric.Create(FabricId.From(++nextId), name, MoneyAmount.From(cost), SupplierId.From(supplierId));
                fabrics.Add(entity);
                db.Fabrics.Add(entity);
                report.ReferenceChanges.Add($"Fabric created: {row.Text(0)} -> {entity.Id.Value}.");
            }
            else if (entity.Price.Value != cost || entity.ProviderId.Value != supplierId)
            {
                report.ReferenceChanges.Add($"Fabric {row.Text(0)} ID {entity.Id.Value}: cost {entity.Price.Value} -> {cost}; supplier {entity.ProviderId.Value} -> {supplierId}.");
                entity.Update(entity.Name, MoneyAmount.From(cost), SupplierId.From(supplierId));
            }
            result.Add(row.Text(0), entity.Id.Value);
            mappings.Add("Fabric", row.Text(0), "Fabric", entity.Id.Value);
        }
        return result;
    }
}

