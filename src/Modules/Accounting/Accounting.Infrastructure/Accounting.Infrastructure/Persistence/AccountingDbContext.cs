using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;

namespace Accounting.Infrastructure.Persistence;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options), IAccountingDbContext
{
    public const string Schema = "accounting";

    public DbSet<FinanceAccount> Accounts => Set<FinanceAccount>();
    public DbSet<FinanceCategory> Categories => Set<FinanceCategory>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
    }
}
