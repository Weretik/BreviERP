using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Balances.Shared;

namespace Accounting.Application.Features.Overview.Queries.GetFinanceOverview;

public sealed class GetFinanceOverviewHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceOverviewQuery, Result<FinanceOverviewResponse>>
{
    private const string WithoutCategory = "Без категорії";

    public async ValueTask<Result<FinanceOverviewResponse>> Handle(GetFinanceOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var accounts = await db.Accounts.AsNoTracking().Where(x => x.Active).ToListAsync(cancellationToken);
        var balances = await FinanceBalances.ForAsync(db, accounts, cancellationToken);

        var period = db.Transactions.AsNoTracking()
            .Where(x => x.Status == MovementStatus.Active && x.Date >= query.From && x.Date <= query.To);
        if (query.Scope is { } scope) period = period.Where(x => x.Scope == scope);
        var sums = await period.GroupBy(x => new { x.Currency, x.Type })
            .Select(g => new { g.Key.Currency, g.Key.Type, Sum = g.Sum(x => x.AmountMinor), Count = g.Count() })
            .ToListAsync(cancellationToken);
        var expenses = await period.Where(x => x.Type == CategoryDirection.Expense)
            .GroupBy(x => new { x.Currency, x.CategoryId })
            .Select(g => new { g.Key.Currency, g.Key.CategoryId, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);
        var categories = await db.Categories.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);

        var currencies = accounts.Select(x => x.Currency).Concat(sums.Select(x => x.Currency)).Distinct()
            .OrderBy(x => x == "UAH" ? 0 : 1).ThenBy(x => x)
            .Select(currency =>
            {
                var own = accounts.Where(x => x.Currency == currency).ToList();
                var unconfirmed = query.Scope == LedgerScope.Current ? own.Count(x => !x.CutoverConfirmed) : 0;
                var balance = own.Sum(x => query.Scope switch
                {
                    LedgerScope.LegacyArchive => balances[x.Id].LegacyMinor,
                    LedgerScope.Current => balances[x.Id].CurrentMinor ?? 0,
                    _ => balances[x.Id].TotalMinor,
                });
                var income = sums.SingleOrDefault(x => x.Currency == currency && x.Type == CategoryDirection.Income);
                var expense = sums.SingleOrDefault(x => x.Currency == currency && x.Type == CategoryDirection.Expense);
                var groups = Roots(expenses.Where(x => x.Currency == currency)
                    .Select(x => (x.CategoryId, x.Sum)), categories);
                var top = groups.Take(FinanceOverviewLimits.TopExpenses).ToList();
                var rest = groups.Skip(FinanceOverviewLimits.TopExpenses).ToList();
                return new FinanceOverviewCurrencyResponse(currency, balance, unconfirmed, income?.Sum ?? 0,
                    expense?.Sum ?? 0, (income?.Sum ?? 0) - (expense?.Sum ?? 0), own.Count, income?.Count ?? 0,
                    expense?.Count ?? 0, top, rest.Sum(x => x.AmountMinor), rest.Count);
            }).ToList();

        var reviews = await db.Transactions.AsNoTracking()
            .Where(x => x.Scope == LedgerScope.Current && x.Status == MovementStatus.Active
                && x.Verification != VerificationStatus.Verified)
            .GroupBy(x => x.Verification)
            .Select(g => new { Verification = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        int Reviews(VerificationStatus status) => reviews.SingleOrDefault(x => x.Verification == status)?.Count ?? 0;
        return Result.Success(new FinanceOverviewResponse(query.Scope, query.From, query.To, currencies,
            reviews.Sum(x => x.Count), Reviews(VerificationStatus.NeedsCategory),
            Reviews(VerificationStatus.NeedsCounterpartyReview)));
    }

    /// <summary>Expense sums folded to root categories, largest first; operations without a category form one group.</summary>
    private static List<FinanceOverviewCategoryResponse> Roots(IEnumerable<(FinanceCategoryId? CategoryId, long Sum)> rows,
        IReadOnlyDictionary<FinanceCategoryId, FinanceCategory> categories) =>
        rows.Select(row =>
            {
                var category = row.CategoryId is { } id ? categories.GetValueOrDefault(id) : null;
                var root = category?.ParentId is { } parent ? categories.GetValueOrDefault(parent) ?? category : category;
                return (Root: root, row.Sum);
            })
            .GroupBy(x => x.Root?.Id)
            .Select(g => new FinanceOverviewCategoryResponse(g.Key?.Value, g.First().Root?.Name ?? WithoutCategory,
                g.Sum(x => x.Sum)))
            .OrderByDescending(x => x.AmountMinor).ThenBy(x => x.Name)
            .ToList();
}
