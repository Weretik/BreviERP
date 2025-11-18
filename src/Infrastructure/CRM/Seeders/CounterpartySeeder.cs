using System.Text.Json;
using Domain.CRM.Entities;
using Domain.CRM.Enums;
using Domain.CRM.ValueObjects;
using Infrastructure.Common.Contracts;

namespace Infrastructure.CRM.Seeders;

public sealed class CounterpartySeeder(
    CrmDbContext db,
    ILogger<CounterpartySeeder> logger)
    : ISeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await db.Counterparty.AnyAsync(cancellationToken))
        {
            logger.LogInformation("ℹ️ Counterparty already exists, skip seeding.");
            return;
        }

        var path = Path.Combine(AppContext.BaseDirectory, "counterparties.json");
        string json = await File.ReadAllTextAsync(path, cancellationToken);

        var rows = JsonSerializer.Deserialize<List<CounterpartySeedRow>>(json)
                   ?? new List<CounterpartySeedRow>();

        var list = new List<Counterparty>();

        foreach (var row in rows)
        {
            var id = CounterpartyId.From(row.Id);
            var type = CounterpartyType.FromName(row.Type);

            var entity = Counterparty.Create(id, type, row.Name);

            list.Add(entity);
        }

        await db.Counterparty.AddRangeAsync(list, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("✅ Seeded {Count} counterparties", list.Count);
    }
}
