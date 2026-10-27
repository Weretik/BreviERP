using Crm.Infrastructure.Persistence;
using Crm.Application.Customers.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Microsoft.EntityFrameworkCore;
namespace Crm.Infrastructure.Persistence.Customers.Writing;

internal sealed class CustomerWriteSession(CrmDbContext db) : ICustomerWriteSession
{
    public async Task<Customer?> LoadAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var roots = await db.Customers.FromSqlInterpolated($"SELECT * FROM crm.\"Customers\" WHERE \"PublicId\" = {publicId} FOR UPDATE").ToListAsync(cancellationToken);
        var customer = roots.SingleOrDefault();
        if (customer is null) return null;
        await db.Entry(customer).Collection(x => x.Contacts).Query().Include(x => x.Channels).LoadAsync(cancellationToken);
        await db.Entry(customer).Collection(x => x.ExternalIdentities).LoadAsync(cancellationToken);
        return customer;
    }

    public void Add(Customer customer) => db.Customers.Add(customer);

    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        db.ChangeTracker.DetectChanges();
        // Release the old unique primary slot before EF orders inserts/updates.
        foreach (var entry in db.ChangeTracker.Entries<CustomerContact>().Where(x => x.State == EntityState.Modified))
        {
            if (entry.Property(x => x.IsPrimary).OriginalValue && !entry.Entity.IsPrimary)
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE crm.\"CustomerContacts\" SET \"IsPrimary\" = false WHERE \"Id\" = {entry.Entity.Id.Value}", cancellationToken);
        }
        await db.SaveChangesAsync(cancellationToken);
    }
}
