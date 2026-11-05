using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;

namespace Accounting.Application.Abstractions;

/// <summary>Persistence port of the Accounting module («Бухгалтерія»); implemented by AccountingDbContext.</summary>
public interface IAccountingDbContext
{
    DbSet<FinanceAccount> Accounts { get; }
    DbSet<FinanceCategory> Categories { get; }
    DbSet<FinanceTransaction> Transactions { get; }
    DbSet<FinanceTransfer> Transfers { get; }
    DbSet<FinanceCounterparty> Counterparties { get; }
    DbSet<AuditEntry> AuditEntries { get; }

    /// <summary>Saves; a concurrent change of the same version throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
