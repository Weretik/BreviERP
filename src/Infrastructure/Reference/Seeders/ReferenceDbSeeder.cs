using Domain.Reference.Entities;
using Infrastructure.Common.Contracts;

namespace Infrastructure.Reference.Seeders;

public sealed class AdditionalReferenceSeeder(
    ReferenceDbContext db,
    ILogger<AdditionalReferenceSeeder> logger)
    : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.AdditionalReferences.AnyAsync(cancellationToken))
        {
            logger.LogInformation("ℹ️ AdditionalReference already exists, skip seeding.");
            return;
        }

        var entity = AdditionalReference.CreateDefault();

        await db.AdditionalReferences.AddAsync(entity, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("✅ Seeded default AdditionalReference with Id={Id}", entity.Id);
    }
}
