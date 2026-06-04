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

            var entity = Supplier.Create(SupplierId.From(row.Id), name, row.Link);

            await db.Suppliers.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded Supplier. Added: {AddedCount}", rows.Count);
    }
}



