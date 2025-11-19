using Domain.Reference.Entities;
using Domain.Reference.ValueObjects;
using Infrastructure.Common.Contracts;

namespace Infrastructure.Reference.Seeders;

public sealed class AdditionalReferenceSeeder(
    ReferenceDbContext db,
    ILogger<AdditionalReferenceSeeder> logger,
    IWebHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.AdditionalReferences.AnyAsync(cancellationToken))
        {
            logger.LogInformation("ℹ️ AdditionalReference already exists, skip seeding.");
            return;
        }

        var path = Path.Combine(env.WebRootPath, "Seed", "additional_reference.json");
        string json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<AdditionalSeedRow>>(json)
                   ?? new List<AdditionalSeedRow>();

        var list = new List<AdditionalReference>();

        foreach (var row in rows)
        {
            var id = AdditionalReferenceId.From(row.Id);
            var entity = AdditionalReference.Create(id, row.Name, row.Value, row.Unit);

            list.Add(entity);
        }

        await db.AdditionalReferences.AddRangeAsync(list, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("✅ Seeded {Count} AdditionalReference", list.Count);
    }
}
