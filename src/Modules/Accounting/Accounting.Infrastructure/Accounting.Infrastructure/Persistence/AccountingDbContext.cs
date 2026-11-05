using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;

namespace Accounting.Infrastructure.Persistence;

public sealed class AccountingDbContext(DbContextOptions<AccountingDbContext> options) : DbContext(options), IAccountingDbContext
{
    public const string Schema = "accounting";

    public DbSet<FinanceAccount> Accounts => Set<FinanceAccount>();
    public DbSet<FinanceCategory> Categories => Set<FinanceCategory>();
    public DbSet<FinanceTransaction> Transactions => Set<FinanceTransaction>();
    public DbSet<FinanceTransfer> Transfers => Set<FinanceTransfer>();
    public DbSet<FinanceCounterparty> Counterparties => Set<FinanceCounterparty>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
    }
}
