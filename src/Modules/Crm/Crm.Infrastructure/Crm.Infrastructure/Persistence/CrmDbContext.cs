using Crm.Application.Quotes.Persistence;
using Crm.Application.Tasks.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Quotes;
using Crm.Domain.Tasks;
using Crm.Infrastructure.Persistence.Quotes;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence;

public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options), IQuoteStore, ITaskStore
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> Contacts => Set<CustomerContact>();
    public DbSet<CustomerMutationRecord> MutationRequests => Set<CustomerMutationRecord>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<ManagerTask> ManagerTasks => Set<ManagerTask>();

    Task<int> IQuoteStore.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    Task<int> ITaskStore.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    public Task<string?> FindCustomerNameAsync(Guid publicId, CancellationToken cancellationToken) =>
        Customers.AsNoTracking()
            .Where(x => x.PublicId == publicId && x.Active && x.MergedIntoId == null)
            .Select(x => x.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);

    public bool IsDuplicateNumber(DbUpdateException exception) => QuoteStoreErrors.IsDuplicateNumber(exception);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("crm");
        builder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
    }
}
