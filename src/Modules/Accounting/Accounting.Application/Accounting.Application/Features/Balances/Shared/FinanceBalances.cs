using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Application.Features.Accounts.Shared;

namespace Accounting.Application.Features.Balances.Shared;

/// <summary>Counts account balances and usage from operations and transfers; nothing is stored.</summary>
internal static class FinanceBalances
{
    /// <summary>Balances of the given accounts, by account id.</summary>
    public static async Task<IReadOnlyDictionary<FinanceAccountId, FinanceAccountBalanceResponse>> ForAsync(
        IAccountingDbContext db, IReadOnlyCollection<FinanceAccount> accounts, CancellationToken cancellationToken)
    {
        var ids = accounts.Select(x => x.Id).ToList();
        var transactions = await db.Transactions.AsNoTracking()
            .Where(x => ids.Contains(x.AccountId))
            .GroupBy(x => new { x.AccountId, x.Scope, x.Type, x.Status })
            .Select(g => new { g.Key.AccountId, g.Key.Scope, g.Key.Type, g.Key.Status, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);
        var outgoing = await db.Transfers.AsNoTracking()
            .Where(x => ids.Contains(x.FromAccountId))
            .GroupBy(x => new { x.FromAccountId, x.Scope, x.Status })
            .Select(g => new { AccountId = g.Key.FromAccountId, g.Key.Scope, g.Key.Status, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);
        var incoming = await db.Transfers.AsNoTracking()
            .Where(x => ids.Contains(x.ToAccountId))
            .GroupBy(x => new { x.ToAccountId, x.Scope, x.Status })
            .Select(g => new { AccountId = g.Key.ToAccountId, g.Key.Scope, g.Key.Status, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);

        var flows = transactions
            .Select(x => new Flow(x.AccountId, x.Scope, x.Status, x.Type == CategoryDirection.Income ? x.Sum : -x.Sum))
            .Concat(outgoing.Select(x => new Flow(x.AccountId, x.Scope, x.Status, -x.Sum)))
            .Concat(incoming.Select(x => new Flow(x.AccountId, x.Scope, x.Status, x.Sum)))
            .ToLookup(x => x.AccountId);

        return accounts.ToDictionary(x => x.Id, account =>
        {
            var rows = flows[account.Id].ToList();
            long Net(LedgerScope? scope) => rows
                .Where(x => x.Status == MovementStatus.Active && (scope is null || x.Scope == scope)).Sum(x => x.Net);
            return new FinanceAccountBalanceResponse(
                account.OpeningMinor + Net(LedgerScope.LegacyArchive),
                account.CutoverConfirmed ? (account.CutoverBalanceMinor ?? 0) + Net(LedgerScope.Current) : null,
                account.OpeningMinor + Net(null),
                rows.Count > 0);
        });
    }

    public static async Task<FinanceAccountBalanceResponse> ForAsync(IAccountingDbContext db, FinanceAccount account,
        CancellationToken cancellationToken) => (await ForAsync(db, [account], cancellationToken))[account.Id];

    /// <summary>Whether any movement (cancelled ones too) uses the account, and the earliest date among them.</summary>
    public static async Task<FinanceAccountUsage> UsageAsync(IAccountingDbContext db, FinanceAccountId id,
        CancellationToken cancellationToken)
    {
        var dates = new List<DateOnly>();
        if (await db.Transactions.AnyAsync(x => x.AccountId == id, cancellationToken))
            dates.Add(await db.Transactions.Where(x => x.AccountId == id).MinAsync(x => x.Date, cancellationToken));
        if (await db.Transfers.AnyAsync(x => x.FromAccountId == id || x.ToAccountId == id, cancellationToken))
            dates.Add(await db.Transfers.Where(x => x.FromAccountId == id || x.ToAccountId == id)
                .MinAsync(x => x.Date, cancellationToken));
        return dates.Count == 0 ? FinanceAccountUsage.None : new FinanceAccountUsage(true, dates.Min());
    }

    private sealed record Flow(FinanceAccountId AccountId, LedgerScope Scope, MovementStatus Status, long Net);
}
