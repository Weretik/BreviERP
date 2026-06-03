using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Seeders.GarmentParts.Rows;
using Microsoft.Extensions.Hosting;

namespace Reference.Infrastructure.Seeders.GarmentParts;

public sealed class GarmentPartSeeder(
    ReferenceDbContext db,
    ILogger<GarmentPartSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "GarmentParts", "Data", "garment_part.json");
        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<GarmentPartSeedRow>>(json)
                   ?? new List<GarmentPartSeedRow>();

        var existingById = await db.GarmentParts
            .ToDictionaryAsync(x => x.Id.Value, cancellationToken);

        var added = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            var name = row.Name.Trim();

            if (existingById.TryGetValue(row.Id, out var existing))
            {
                existing.Update(name);
                updated++;
                continue;
            }

            var entity = GarmentPart.Create(GarmentPartId.From(row.Id), name);

            await db.GarmentParts.AddAsync(entity, cancellationToken);
            added++;
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded GarmentPart. Added: {AddedCount}, Updated: {UpdatedCount}",
            added,
            updated);
    }
}



