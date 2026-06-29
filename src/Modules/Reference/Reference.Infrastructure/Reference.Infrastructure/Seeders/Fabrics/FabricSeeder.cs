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

            var entity = Fabric.Create(FabricId.From(row.Id), name, price, providerId);

            await db.Fabrics.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded Fabric. Added: {AddedCount}", rows.Count);
    }
}




