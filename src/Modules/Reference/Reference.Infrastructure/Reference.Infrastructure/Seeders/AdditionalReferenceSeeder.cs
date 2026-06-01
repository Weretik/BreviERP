using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace Reference.Infrastructure.Seeders;

public sealed class AdditionalReferenceSeeder(
    ReferenceDbContext db,
    ILogger<AdditionalReferenceSeeder> logger,
    IWebHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "Data", "additional_reference.json");
        string json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<AdditionalSeedRow>>(json)
                   ?? new List<AdditionalSeedRow>();

        var existingById = await db.AdditionalReferences
            .ToDictionaryAsync(x => x.Id.Value, cancellationToken);

        var added = 0;
        var updated = 0;

        foreach (var row in rows)
        {
            var id = AdditionalReferenceId.From(row.Id);

            if (existingById.TryGetValue(row.Id, out var existing))
            {
                existing.Update(row.Name, row.Key, row.Value, row.Unit, row.Description);
                updated++;
                continue;
            }

            var entity = AdditionalReference.Create(id, row.Name, row.Key, row.Value, row.Unit, row.Description);

            await db.AdditionalReferences.AddAsync(entity, cancellationToken);
            added++;
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Seeded AdditionalReference. Added: {AddedCount}, Updated: {UpdatedCount}",
            added,
            updated);
    }
}
