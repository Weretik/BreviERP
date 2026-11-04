using Crm.Application.Customers.KeepinCrmSync;
using Crm.Application.Embroidery.Persistence;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Tasks.Persistence;
using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Domain.Embroidery.Saved;
using Crm.Domain.Embroidery.Settings;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Introductions;
using Crm.Domain.Quotes.Sending;
using Crm.Domain.Quotes.Settings;
using Crm.Domain.Tasks;
using Crm.Infrastructure.Persistence.Quotes;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence;

public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options), IQuoteStore, ITaskStore, IEmbroideryStore
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> Contacts => Set<CustomerContact>();
    public DbSet<CustomerMutationRecord> MutationRequests => Set<CustomerMutationRecord>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteSettings> QuoteSettings => Set<QuoteSettings>();
    public DbSet<StaffMailbox> StaffMailboxes => Set<StaffMailbox>();
    public DbSet<QuoteSending> QuoteSendings => Set<QuoteSending>();
    public DbSet<QuoteEvent> QuoteEvents => Set<QuoteEvent>();
    public DbSet<QuoteIntroTemplate> QuoteIntroTemplates => Set<QuoteIntroTemplate>();
    public DbSet<ManagerTask> ManagerTasks => Set<ManagerTask>();
    public DbSet<EmbroiderySettings> EmbroiderySettings => Set<EmbroiderySettings>();
    public DbSet<EmbroiderySavedCalculation> EmbroideryCalculations => Set<EmbroiderySavedCalculation>();

    Task<int> IQuoteStore.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    Task<int> ITaskStore.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    Task<int> IEmbroideryStore.SaveChangesAsync(CancellationToken cancellationToken) => SaveChangesAsync(cancellationToken);

    public Task<string?> FindCustomerNameAsync(Guid publicId, CancellationToken cancellationToken) =>
        Customers.AsNoTracking()
            .Where(x => x.PublicId == publicId && x.Active && x.MergedIntoId == null)
            .Select(x => x.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<string, long>> FindKeepinCustomersAsync(
        IReadOnlyCollection<string> keepinIds, CancellationToken cancellationToken)
    {
        if (keepinIds.Count == 0) return new Dictionary<string, long>();
        var rows = await Customers.AsNoTracking()
            .Where(x => x.Active && x.MergedIntoId == null)
            .SelectMany(x => x.ExternalIdentities
                .Where(i => i.Source == KeepinCrmClientIdentity.Source && i.Type == KeepinCrmClientIdentity.Type &&
                    keepinIds.Contains(i.ExternalId))
                .Select(i => new { i.ExternalId, i.CustomerId }))
            .ToListAsync(cancellationToken);
        return rows.GroupBy(x => x.ExternalId).ToDictionary(x => x.Key, x => x.First().CustomerId.Value);
    }

    public bool IsDuplicateNumber(DbUpdateException exception) => QuoteStoreErrors.IsDuplicateNumber(exception);

    public bool IsDuplicateSettings(DbUpdateException exception) => QuoteStoreErrors.IsDuplicateSettings(exception);

    public bool IsDuplicateMailbox(DbUpdateException exception) => QuoteStoreErrors.IsDuplicateMailbox(exception);

    public bool IsDuplicateEmbroiderySettings(DbUpdateException exception) =>
        QuoteStoreErrors.IsPrimaryKeyViolation(exception, "PK_EmbroiderySettings");

    public bool IsDuplicateTemplateName(DbUpdateException exception) =>
        QuoteStoreErrors.IsDuplicateTemplateName(exception);

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("crm");
        builder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
    }
}
