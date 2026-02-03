using BuildingBlocks.Infrastructure.Seeding;
using Infrastructure.Reference.Seeders;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Seeders;

public sealed class FabricSeeder(
    ReferenceDbContext db,
    ILogger<FabricSeeder> logger,
    IWebHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Fabrics.AnyAsync(cancellationToken))
        {
            logger.LogInformation("ℹ️ Fabric already exists, skip seeding.");
            return;
        }

        var path = Path.Combine(env.WebRootPath, "Seed", "fabric.json");
        string json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<FabricSeedRow>>(json)
                   ?? new List<FabricSeedRow>();

        var list = new List<Fabric>();

        foreach (var row in rows)
        {
            var id = FabricId.From(row.Id);
            var price = new Money(row.Price, "UAH");
            var entity = Fabric.Create(id, row.Name, row.CounterpartyId, price);

            list.Add(entity);
        }

        await db.Fabrics.AddRangeAsync(list, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("✅ Seeded {Count} Fabric", list.Count);
    }
}
