using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
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
        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<SupplierSeedRow>>(json)
                   ?? new List<SupplierSeedRow>();

        var existingById = await db.Suppliers
            .ToDictionaryAsync(x => x.Id.Value, cancellationToken);

        var added = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            var name = row.Name.Trim();

            if (existingById.TryGetValue(row.Id, out var existing))
            {
                existing.Update(name, row.Link);
                updated++;
                continue;
            }

            var entity = Supplier.Create(SupplierId.From(row.Id), name, row.Link);

            await db.Suppliers.AddAsync(entity, cancellationToken);
            added++;
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded Supplier. Added: {AddedCount}, Updated: {UpdatedCount}",
            added,
            updated);
    }
}



