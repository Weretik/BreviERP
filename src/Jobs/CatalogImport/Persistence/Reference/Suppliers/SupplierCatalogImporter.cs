using CatalogImport.Execution;
using CatalogImport.Source;
using Microsoft.EntityFrameworkCore;
using Reference.Domain.Suppliers.Entities;
using Reference.Domain.Suppliers.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace CatalogImport.Persistence;

public sealed class SupplierCatalogImporter(
    ReferenceDbContext db, SourceMappingWriter mappings, CatalogImportReport report, List<Supplier> suppliers)
{
    private int _nextId = suppliers.Select(x => x.Id.Value).DefaultIfEmpty().Max();
    public Dictionary<string, int> Resolved { get; } = new(StringComparer.Ordinal);

    public int Resolve(string name)
    {
        if (name.Length == 0)
            return (suppliers.SingleOrDefault(x => x.Id.Value == 1 && x.Name == "Не указан")
                ?? throw new InvalidDataException("The expected unspecified supplier is absent.")).Id.Value;
        if (Resolved.TryGetValue(name, out var id))
            return id;
        var supplier = suppliers.SingleOrDefault(x => x.Name == name);
        if (supplier is null)
        {
            supplier = Supplier.Create(SupplierId.From(++_nextId), name);
            suppliers.Add(supplier);
            db.Suppliers.Add(supplier);
            report.ReferenceChanges.Add($"Supplier created: {name} -> {supplier.Id.Value}.");
        }
        Resolved.Add(name, supplier.Id.Value);
        mappings.Add("SupplierName", name, "Supplier", supplier.Id.Value);
        return supplier.Id.Value;
    }

    public void Import(WorkbookSnapshot snapshot)
    {
        foreach (var row in snapshot.Rows("Постачальники"))
        {
            var id = Resolve(row.Text(1));
            var supplier = suppliers.Single(x => x.Id.Value == id);
            if (supplier.Link != Empty(row.Text(6)) || supplier.ContactPerson != Empty(row.Text(3))
                || supplier.PhoneNumber != Empty(row.Text(4)))
                report.ReferenceChanges.Add($"Supplier contact fields updated from {row.Key}: ID {id}.");
            supplier.Update(row.Text(1), row.Text(6), row.Text(3), row.Text(4), supplier.Notes);
            mappings.Add("Supplier", row.Text(0), "Supplier", id);
        }
    }

    private static string? Empty(string value) => value.Length == 0 ? null : value;
}

