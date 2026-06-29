using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.AdditionalReferences.Entities;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Products.Entities;
using Reference.Domain.Suppliers.Entities;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Seeders.Suppliers.Rows;
using Microsoft.Extensions.Hosting;

namespace Reference.Infrastructure.Seeders.Suppliers;

public sealed class SupplierSeeder(
    ReferenceDbContext db,
    ILogger<SupplierSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "Suppliers", "Data", "supplier.json");
        if (await db.Suppliers.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped Supplier seeding because table already contains data.");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<SupplierSeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<SupplierSeedRow>();

        foreach (var row in rows)
        {
            var name = row.Name.Trim();
            var link = string.IsNullOrWhiteSpace(row.Link) ? null : row.Link.Trim();
            var contactPerson = string.IsNullOrWhiteSpace(row.ContactPerson) ? null : row.ContactPerson.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(row.PhoneNumber) ? null : row.PhoneNumber.Trim();
            var notes = string.IsNullOrWhiteSpace(row.Notes) ? null : row.Notes.Trim();

            var entity = Supplier.Create(
                SupplierId.From(row.Id),
                name,
                link,
                contactPerson,
                phoneNumber,
                notes);

            await db.Suppliers.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded Supplier. Added: {AddedCount}", rows.Count);
    }
}



