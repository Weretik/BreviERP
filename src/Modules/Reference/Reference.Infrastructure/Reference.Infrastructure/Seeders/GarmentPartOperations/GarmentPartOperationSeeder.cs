using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Seeders.GarmentPartOperations.Rows;

namespace Reference.Infrastructure.Seeders.GarmentPartOperations;

public sealed class GarmentPartOperationSeeder(
    ReferenceDbContext db,
    ILogger<GarmentPartOperationSeeder> logger,
    IWebHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "GarmentPartOperations", "Data", "garment_part_operation.json");
        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<GarmentPartOperationSeedRow>>(json)
                   ?? new List<GarmentPartOperationSeedRow>();

        var existingById = await db.GarmentPartOperations
            .ToDictionaryAsync(x => x.Id.Value, cancellationToken);

        var added = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            var name = row.Name.Trim();
            var garmentPartId = GarmentPartId.From(row.GarmentPartId);

            if (existingById.TryGetValue(row.Id, out var existing))
            {
                existing.Update(garmentPartId, name, row.Min);
                updated++;
                continue;
            }

            var entity = GarmentPartOperation.Create(
                GarmentPartOperationId.From(row.Id),
                garmentPartId,
                name,
                row.Min);

            await db.GarmentPartOperations.AddAsync(entity, cancellationToken);
            added++;
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded GarmentPartOperation. Added: {AddedCount}, Updated: {UpdatedCount}",
            added,
            updated);
    }
}
