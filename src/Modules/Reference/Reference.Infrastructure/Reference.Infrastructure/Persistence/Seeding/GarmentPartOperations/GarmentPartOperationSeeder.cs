using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Microsoft.Extensions.Hosting;
using Reference.Infrastructure.Persistence;

namespace Reference.Infrastructure.Persistence.Seeding.GarmentPartOperations;

public sealed class GarmentPartOperationSeeder(
    ReferenceDbContext db,
    ILogger<GarmentPartOperationSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Persistence", "Seeding", "GarmentPartOperations", "Data", "garment_part_operation.json");
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

            var created = GarmentPartOperation.Create(
                GarmentPartOperationId.From(row.Id),
                new GarmentPartOperationDetails(row.Code, garmentPartId, row.Element, row.Variant, name, row.Min,
                    row.MinutesPerProduct, row.IsActive, row.Notes));
            if (created.IsFailure)
            {
                logger.LogWarning("Skipped GarmentPartOperation seed row {Id}: {Error}", row.Id, created.Error!.Message);
                continue;
            }

            var entity = created.Value;

            await db.GarmentPartOperations.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded GarmentPartOperation. Added: {AddedCount}", rows.Count);
    }
}



