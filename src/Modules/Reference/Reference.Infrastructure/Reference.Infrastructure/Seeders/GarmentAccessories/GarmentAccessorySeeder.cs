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
using Reference.Infrastructure.Seeders.GarmentAccessories.Rows;
using Microsoft.Extensions.Hosting;

namespace Reference.Infrastructure.Seeders.GarmentAccessories;

public sealed class GarmentAccessorySeeder(
    ReferenceDbContext db,
    ILogger<GarmentAccessorySeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "GarmentAccessories", "Data", "garment-accessory.json");
        if (await db.GarmentAccessories.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped GarmentAccessory seeding because table already contains data.");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<GarmentAccessorySeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<GarmentAccessorySeedRow>();

        foreach (var row in rows)
        {
            if (row is null)
            {
                logger.LogWarning("Skipped empty GarmentAccessory seed row.");
                continue;
            }

            var name = row.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                logger.LogWarning("Skipped GarmentAccessory seed row {Id} because name is empty.", row.Id);
                continue;
            }

            var price = MoneyAmount.From(row.Price);

            var entity = GarmentAccessory.Create(GarmentAccessoryId.From(row.Id), name, price);

            await db.GarmentAccessories.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded GarmentAccessory. Added: {AddedCount}", rows.Count);
    }
}



