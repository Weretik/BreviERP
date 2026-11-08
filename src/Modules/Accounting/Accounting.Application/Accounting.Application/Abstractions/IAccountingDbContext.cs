using Accounting.Domain.Accounts;
using Accounting.Domain.Attachments;
using Accounting.Domain.Audit;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Transactions;
using Accounting.Domain.Transfers;
using Microsoft.EntityFrameworkCore.Storage;

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
    DbSet<FinanceAttachment> Attachments { get; }

    /// <summary>Saves; a concurrent change of the same version throws <see cref="DbUpdateConcurrencyException"/>.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);

    /// <summary>Opens a database transaction; the archive move (stage 5) saves in parts inside one.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    /// <summary>Forgets the tracked records once a part is saved, so a big move does not keep them all in memory.</summary>
    void ClearTracked();
}
