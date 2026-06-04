using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
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



