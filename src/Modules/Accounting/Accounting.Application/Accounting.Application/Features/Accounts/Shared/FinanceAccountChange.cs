using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Audit;
using Accounting.Application;
using Accounting.Application.Features.Audit.Shared;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Accounts.Shared;

/// <summary>
/// Shared steps of every account change: find it, check the version the user saw, apply the change, write the log entry
/// with snapshots and save. A change that alters nothing keeps the version and writes no entry.
/// </summary>
internal static class FinanceAccountChange
{
    /// <param name="change">
    /// Applies the change and returns its log action, null when nothing changed, or the rule it broke.
    /// </param>
    public static async Task<Result<FinanceAccountResponse>> ApplyAsync(IAccountingDbContext db, Guid id, int version,
        string by, DateTimeOffset now, Func<FinanceAccount, DomainResult<AuditAction?>> change, CancellationToken cancellationToken)
    {
        var account = await db.Accounts.FirstOrDefaultAsync(x => x.Id == FinanceAccountId.From(id), cancellationToken);
        if (account is null) return Result.NotFound(AccountingMessages.AccountNotFound);
        if (account.Version != version) return Result.Conflict(AccountingMessages.Stale);

        var before = AuditTrail.Snapshot(account);
        var changed = change(account);
        if (changed.IsFailure) return changed.Error!.ToResult<FinanceAccountResponse>();
        var action = changed.Value;
        if (action is null) return await ResponseAsync(db, account, cancellationToken);

        AuditTrail.Add(db, AuditEntity.Account, id, action.Value, by, now, before, AuditTrail.Snapshot(account));
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(AccountingMessages.Stale);
        }

        return await ResponseAsync(db, account, cancellationToken);
    }

    public static async Task<Result<FinanceAccountResponse>> ResponseAsync(IAccountingDbContext db, FinanceAccount account,
        CancellationToken cancellationToken) =>
        Result.Success(FinanceAccountResponse.From(account, await FinanceBalances.ForAsync(db, account, cancellationToken)));
}
