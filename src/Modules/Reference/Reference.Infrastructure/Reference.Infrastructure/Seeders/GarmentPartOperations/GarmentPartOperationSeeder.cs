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
using Reference.Infrastructure.Seeders.GarmentPartOperations.Rows;
using Microsoft.Extensions.Hosting;

namespace Reference.Infrastructure.Seeders.GarmentPartOperations;

public sealed class GarmentPartOperationSeeder(
    ReferenceDbContext db,
    ILogger<GarmentPartOperationSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "GarmentPartOperations", "Data", "garment_part_operation.json");
        if (await db.GarmentPartOperations.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped GarmentPartOperation seeding because table already contains data.");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<GarmentPartOperationSeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<GarmentPartOperationSeedRow>();

        foreach (var row in rows)
        {
            var name = row.Name.Trim();
            var garmentPartId = GarmentPartId.From(row.GarmentPartId);

            var entity = GarmentPartOperation.Create(
                GarmentPartOperationId.From(row.Id),
                garmentPartId,
                name,
                row.Min);

            await db.GarmentPartOperations.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded GarmentPartOperation. Added: {AddedCount}", rows.Count);
    }
}



