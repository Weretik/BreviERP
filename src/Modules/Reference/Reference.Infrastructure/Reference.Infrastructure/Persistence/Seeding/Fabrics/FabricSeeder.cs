using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Microsoft.Extensions.Hosting;
using Reference.Infrastructure.Persistence;
using Reference.Domain.Fabrics;
using BuildingBlocks.Domain;

namespace Reference.Infrastructure.Persistence.Seeding.Fabrics;

public sealed class FabricSeeder(
    ReferenceDbContext db,
    ILogger<FabricSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Persistence", "Seeding", "Fabrics", "Data", "fabric.json");
        if (await db.Fabrics.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped Fabric seeding because table already contains data.");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<FabricSeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<FabricSeedRow>();

        foreach (var row in rows)
        {
            var name = row.Name.Trim();
            var price = MoneyAmount.From(row.Price);
            var providerId = SupplierId.From(row.ProviderId);

            var created = Fabric.Create(FabricId.From(row.Id), name, price, providerId);
            if (created.IsFailure)
            {
                logger.LogWarning("Skipped Fabric seed row {Id}: {Error}", row.Id, created.Error!.Message);
                continue;
            }

            var entity = created.Value;

            await db.Fabrics.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded Fabric. Added: {AddedCount}", rows.Count);
    }
}




