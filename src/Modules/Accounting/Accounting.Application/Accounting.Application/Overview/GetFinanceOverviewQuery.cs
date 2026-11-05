using Accounting.Application.Abstractions;
using Accounting.Application.Balances;
using Accounting.Application.Contracts;
using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;

namespace Accounting.Application.Overview;

/// <summary>«Огляд»: balances, income and expense per currency for an area (<paramref name="Scope"/> null — both).</summary>
public sealed record GetFinanceOverviewQuery(LedgerScope? Scope, DateOnly From, DateOnly To)
    : IQuery<Result<FinanceOverviewResponse>>;

public sealed class GetFinanceOverviewQueryValidator : AbstractValidator<GetFinanceOverviewQuery>
{
    public GetFinanceOverviewQueryValidator()
    {
        RuleFor(x => x.Scope).IsInEnum().When(x => x.Scope is not null);
        RuleFor(x => x.To).GreaterThanOrEqualTo(x => x.From).WithMessage("Кінець періоду раніший за початок.");
    }
}

public sealed class GetFinanceOverviewQueryHandler(IAccountingDbContext db)
    : IQueryHandler<GetFinanceOverviewQuery, Result<FinanceOverviewResponse>>
{
    public async ValueTask<Result<FinanceOverviewResponse>> Handle(GetFinanceOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var accounts = await db.Accounts.AsNoTracking().Where(x => x.Active).ToListAsync(cancellationToken);
        var balances = await FinanceBalances.ForAsync(db, accounts, cancellationToken);

        var period = db.Transactions.AsNoTracking()
            .Where(x => x.Status == MovementStatus.Active && x.Date >= query.From && x.Date <= query.To);
        if (query.Scope is { } scope) period = period.Where(x => x.Scope == scope);
        var sums = await period.GroupBy(x => new { x.Currency, x.Type })
            .Select(g => new { g.Key.Currency, g.Key.Type, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);

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
                var income = sums.Where(x => x.Currency == currency && x.Type == CategoryDirection.Income).Sum(x => x.Sum);
                var expense = sums.Where(x => x.Currency == currency && x.Type == CategoryDirection.Expense).Sum(x => x.Sum);
                return new FinanceOverviewCurrencyResponse(currency, balance, unconfirmed, income, expense, income - expense,
                    own.Count);
            }).ToList();

        return Result.Success(new FinanceOverviewResponse(query.Scope, query.From, query.To, currencies));
    }
}
