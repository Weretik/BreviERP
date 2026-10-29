using Crm.Domain.Customers;
using Crm.Domain.Customers.Contacts;
using Crm.Infrastructure.Persistence.Quotes.Pricing;
using Crm.Domain.Quotes;
using Crm.Domain.Quotes.Versions;
using Crm.Domain.Quotes.Issues;
using Crm.Infrastructure.Persistence.Quotes.Writing;
using Microsoft.EntityFrameworkCore;

namespace Crm.Infrastructure.Persistence;

public sealed class CrmDbContext(DbContextOptions<CrmDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerContact> Contacts => Set<CustomerContact>();
    public DbSet<CustomerMutationRecord> MutationRequests => Set<CustomerMutationRecord>();
    public DbSet<QuotePriceBasisRecord> CalculationBases => Set<QuotePriceBasisRecord>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteVersion> QuoteVersions => Set<QuoteVersion>();
    public DbSet<QuoteIssue> QuoteIssues => Set<QuoteIssue>();
    public DbSet<QuoteMutationRecord> QuoteMutationRequests => Set<QuoteMutationRecord>();

    public DbSet<Crm.Domain.Documents.DocumentSettingsState> DocumentSettingsStates => Set<Crm.Domain.Documents.DocumentSettingsState>();
    public DbSet<Crm.Domain.Documents.Issuer.IssuerSettingsVersion> IssuerSettingsVersions => Set<Crm.Domain.Documents.Issuer.IssuerSettingsVersion>();
    public DbSet<Crm.Domain.Documents.Templates.DocumentTemplateVersion> DocumentTemplateVersions => Set<Crm.Domain.Documents.Templates.DocumentTemplateVersion>();
    public DbSet<Crm.Infrastructure.Persistence.Documents.DocumentSettingsMutationRecord> DocumentSettingsMutations => Set<Crm.Infrastructure.Persistence.Documents.DocumentSettingsMutationRecord>();
    public DbSet<Crm.Infrastructure.Persistence.Documents.DocumentNumberSequence> DocumentNumberSequences => Set<Crm.Infrastructure.Persistence.Documents.DocumentNumberSequence>();

    public DbSet<Crm.Domain.CustomSewing.Drafts.CustomSewingDraft> CustomDrafts => Set<Crm.Domain.CustomSewing.Drafts.CustomSewingDraft>();
    public DbSet<Crm.Domain.CustomSewing.Versions.CustomSewingVersion> CustomVersions => Set<Crm.Domain.CustomSewing.Versions.CustomSewingVersion>();
    public DbSet<Crm.Infrastructure.Persistence.CustomSewing.Pricing.CustomCalculationRecord> CustomCalculations => Set<Crm.Infrastructure.Persistence.CustomSewing.Pricing.CustomCalculationRecord>();
    public DbSet<Crm.Infrastructure.Persistence.CustomSewing.Writing.CustomMutationRecord> CustomMutations => Set<Crm.Infrastructure.Persistence.CustomSewing.Writing.CustomMutationRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("crm");
        builder.ApplyConfigurationsFromAssembly(typeof(CrmDbContext).Assembly);
    }
}
