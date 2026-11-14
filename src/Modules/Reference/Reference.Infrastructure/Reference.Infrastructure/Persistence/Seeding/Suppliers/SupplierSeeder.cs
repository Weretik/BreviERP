using BuildingBlocks.Infrastructure.Seeding;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Suppliers;
using Microsoft.Extensions.Hosting;
using Reference.Infrastructure.Persistence;

namespace Reference.Infrastructure.Persistence.Seeding.Suppliers;

public sealed class SupplierSeeder(
    ReferenceDbContext db,
    ILogger<SupplierSeeder> logger,
    IHostEnvironment env)
    : ISeeder
{
    public async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(env.ContentRootPath, "Persistence", "Seeding", "Suppliers", "Data", "supplier.json");
        if (await db.Suppliers.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipped Supplier seeding because table already contains data.");
            return;
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<SupplierSeedRow>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })
                   ?? new List<SupplierSeedRow>();

        foreach (var row in rows)
        {
            var name = row.Name.Trim();
            var link = string.IsNullOrWhiteSpace(row.Link) ? null : row.Link.Trim();
            var contactPerson = string.IsNullOrWhiteSpace(row.ContactPerson) ? null : row.ContactPerson.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(row.PhoneNumber) ? null : row.PhoneNumber.Trim();
            var notes = string.IsNullOrWhiteSpace(row.Notes) ? null : row.Notes.Trim();

            var created = Supplier.Create(
                SupplierId.From(row.Id),
                name,
                link,
                contactPerson,
                phoneNumber,
                notes);
            if (created.IsFailure)
            {
                logger.LogWarning("Skipped Supplier seed row {Id}: {Error}", row.Id, created.Error!.Message);
                continue;
            }

            var entity = created.Value;

            await db.Suppliers.AddAsync(entity, cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeded Supplier. Added: {AddedCount}", rows.Count);
    }
}



