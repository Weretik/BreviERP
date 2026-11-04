using Crm.Application.Customers.KeepinCrmSync;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Customers.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence.Customers.KeepinCrmSync;

internal sealed class CustomerKeepinSyncStore(CrmDbContext db) : ICustomerKeepinSyncStore
{
    public async Task<IReadOnlyList<Customer>> ListWithDetailsAsync(CancellationToken cancellationToken) =>
        await db.Customers
            .Where(x => x.MergedIntoId == null)
            .Include(x => x.Contacts).ThenInclude(x => x.Channels)
            .Include(x => x.ExternalIdentities)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlySet<CustomerId>> ListInUseAsync(CancellationToken cancellationToken)
    {
        var quoted = await db.Quotes.Where(x => x.CustomerId != null)
            .Select(x => new CustomerId(x.CustomerId!.Value)).ToListAsync(cancellationToken);
        var mergeTargets = await db.Customers.Where(x => x.MergedIntoId != null).Select(x => x.MergedIntoId!.Value).ToListAsync(cancellationToken);
        return quoted.Concat(mergeTargets).ToHashSet();
    }

    public void Add(Customer customer) => db.Customers.Add(customer);

    /// <summary>Contacts and identities use restrict delete, so they go first; channels cascade with contacts.</summary>
    public void Remove(Customer customer)
    {
        db.Set<CustomerChannel>().RemoveRange(customer.Contacts.SelectMany(x => x.Channels));
        db.Contacts.RemoveRange(customer.Contacts);
        db.Set<Crm.Domain.Customers.ExternalIdentities.CustomerExternalIdentity>().RemoveRange(customer.ExternalIdentities);
        db.Customers.Remove(customer);
    }

    public async Task SaveAsync(DateTimeOffset syncedAt, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.ChangeTracker.DetectChanges();
        // Release an old unique primary slot before EF orders inserts and updates, as CustomerWriteSession does.
        foreach (var entry in db.ChangeTracker.Entries<CustomerContact>().Where(x => x.State == EntityState.Modified))
        {
            if (entry.Property(x => x.IsPrimary).OriginalValue && !entry.Entity.IsPrimary)
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE crm.\"CustomerContacts\" SET \"IsPrimary\" = false WHERE \"Id\" = {entry.Entity.Id.Value}",
                    cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
        await db.Customers
            .Where(x => x.MergedIntoId == null && x.ExternalIdentities.Any(i =>
                i.Source == KeepinCrmClientIdentity.Source && i.Type == KeepinCrmClientIdentity.Type))
            .ExecuteUpdateAsync(x => x.SetProperty(c => c.KeepinSyncedAt, syncedAt), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
