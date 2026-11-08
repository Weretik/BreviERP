using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Statements;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Microsoft.EntityFrameworkCore.Storage;

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
    public DbSet<FinanceAttachment> Attachments => Set<FinanceAttachment>();
    public DbSet<BankStatement> BankStatements => Set<BankStatement>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        Database.BeginTransactionAsync(cancellationToken);

    public void ClearTracked() => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.ApplyConfigurationsFromAssembly(typeof(AccountingDbContext).Assembly);
    }
}
