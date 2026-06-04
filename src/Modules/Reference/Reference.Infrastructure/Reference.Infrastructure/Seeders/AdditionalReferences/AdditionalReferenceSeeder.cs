using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.Entities;
using Reference.Domain.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Seeders.AdditionalReferences.Rows;
using Microsoft.Extensions.Hosting;

namespace Reference.Infrastructure.Seeders.AdditionalReferences;

public sealed class AdditionalReferenceSeeder(
    ReferenceDbContext db,
    ILogger<AdditionalReferenceSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Seeders", "AdditionalReferences", "Data", "additional_reference.json");
        if (await db.AdditionalReferences.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped AdditionalReference seeding because table already contains data.");
            return;
        }

        string json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<AdditionalSeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<AdditionalSeedRow>();

        foreach (var row in rows)
        {
            var entity = AdditionalReference.Create(
                AdditionalReferenceId.From(row.Id),
                row.Name,
                row.Key,
                row.Value,
                row.Unit,
                row.Description);

            await db.AdditionalReferences.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded AdditionalReference. Added: {AddedCount}", rows.Count);
    }
}



