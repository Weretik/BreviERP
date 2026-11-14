using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Microsoft.Extensions.Hosting;
using Reference.Infrastructure.Persistence;

namespace Reference.Infrastructure.Persistence.Seeding.GarmentParts;

public sealed class GarmentPartSeeder(
    ReferenceDbContext db,
    ILogger<GarmentPartSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Persistence", "Seeding", "GarmentParts", "Data", "garment_part.json");
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

            var created = GarmentPart.Create(GarmentPartId.From(row.Id), name);
            if (created.IsFailure)
            {
                logger.LogWarning("Skipped GarmentPart seed row {Id}: {Error}", row.Id, created.Error!.Message);
                continue;
            }

            var entity = created.Value;

            await db.GarmentParts.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded GarmentPart. Added: {AddedCount}", rows.Count);
    }
}



