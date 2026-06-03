using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Seeders.Fabrics.Rows;
using Microsoft.Extensions.Hosting;

namespace Reference.Infrastructure.Seeders.Fabrics;

public sealed class FabricSeeder(
    ReferenceDbContext db,
    ILogger<FabricSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "Fabrics", "Data", "fabric.json");
        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<FabricSeedRow>>(json)
                   ?? new List<FabricSeedRow>();

        var existingById = await db.Fabrics
            .ToDictionaryAsync(x => x.Id.Value, cancellationToken);

        var added = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            var name = row.Name.Trim();
            var price = MoneyAmount.From(row.Price);
            var providerId = SupplierId.From(row.ProviderId);

            if (existingById.TryGetValue(row.Id, out var existing))
            {
                existing.Update(name, price, providerId);
                updated++;
                continue;
            }

            var entity = Fabric.Create(FabricId.From(row.Id), name, price, providerId);

            await db.Fabrics.AddAsync(entity, cancellationToken);
            added++;
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded Fabric. Added: {AddedCount}, Updated: {UpdatedCount}",
            added,
            updated);
    }
}




