using Accounting.Application.Abstractions;
using Accounting.Application.Audit;
using Accounting.Application.Contracts;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;

namespace Accounting.Application.Accounts;

/// <summary>
/// Shared steps of every account change: find it, check the version the user saw, apply the change, write the log entry
/// with snapshots and save. A change that alters nothing keeps the version and writes no entry.
/// </summary>
internal static class FinanceAccountChange
{
    /// <param name="change">Applies the change and returns its log action, or null when nothing changed.</param>
    public static async Task<Result<FinanceAccountResponse>> ApplyAsync(IAccountingDbContext db, Guid id, int version,
        string by, DateTimeOffset now, Func<FinanceAccount, AuditAction?> change, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(x => x.Id == FinanceAccountId.From(id), cancellationToken);
        if (account is null) return Result.NotFound(AccountingMessages.AccountNotFound);
        if (account.Version != version) return Result.Conflict(AccountingMessages.Stale);

        var before = AuditTrail.Snapshot(account);
        var action = change(account);
        if (action is null) return Result.Success(FinanceAccountResponse.From(account));

        AuditTrail.Add(db, AuditEntity.Account, id, action.Value, by, now, before, AuditTrail.Snapshot(account));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return Result.Success(FinanceAccountResponse.From(account));
    }
}
