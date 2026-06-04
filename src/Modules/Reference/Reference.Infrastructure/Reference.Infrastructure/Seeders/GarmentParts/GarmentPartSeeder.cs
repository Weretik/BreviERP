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
        if (await db.GarmentParts.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped GarmentPart seeding because table already contains data.");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<GarmentPartSeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<GarmentPartSeedRow>();

        foreach (var row in rows)
        {
            var name = row.Name.Trim();

            var entity = GarmentPart.Create(GarmentPartId.From(row.Id), name);

            await db.GarmentParts.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded GarmentPart. Added: {AddedCount}", rows.Count);
    }
}



