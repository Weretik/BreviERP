using BuildingBlocks.Domain.ValueObjects;
using CatalogImport.Execution;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace CatalogImport.Persistence;

public sealed class AccessoryCatalogImporter(
    ReferenceDbContext db, SourceMappingWriter mappings, SupplierCatalogImporter suppliers, CatalogImportReport report)
{
    public async Task<Dictionary<string, int>> ImportAsync(WorkbookSnapshot snapshot, CancellationToken cancellationToken)
    {
        var accessories = await db.GarmentAccessories.ToListAsync(cancellationToken);
        var nextId = accessories.Select(x => x.Id.Value).DefaultIfEmpty().Max();
        var result = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var row in snapshot.Rows("Фурнітура"))
        {
            if (row.Text(4) == "послуга")
            {
                mappings.Add("OtherCost", row.Text(0), "SourceCostCode", 0);
                continue;
            }
            var supplierId = suppliers.Resolve(row.Text(2));
            var entity = accessories.SingleOrDefault(x => x.Name == row.Text(1));
            var cost = row.Decimal(3)!.Value;
            if (entity is null)
            {
                entity = GarmentAccessory.Create(GarmentAccessoryId.From(++nextId), row.Text(1), MoneyAmount.From(cost), supplierId);
                accessories.Add(entity);
                db.GarmentAccessories.Add(entity);
                report.ReferenceChanges.Add($"Accessory created: {row.Text(0)} -> {entity.Id.Value}.");
            }
            else if (entity.Price.Value != cost || entity.SupplierId.Value != supplierId)
            {
                report.ReferenceChanges.Add($"Accessory {row.Text(0)} ID {entity.Id.Value}: cost {entity.Price.Value} -> {cost}; supplier {entity.SupplierId.Value} -> {supplierId}.");
                entity.Update(entity.Name, MoneyAmount.From(cost), supplierId);
            }
            result.Add(row.Text(0), entity.Id.Value);
            mappings.Add("Accessory", row.Text(0), "Accessory", entity.Id.Value);
        }
        return result;
    }
}

