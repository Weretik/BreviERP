using Accounting.Application.Abstractions;
using Accounting.Domain.Accounts;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Transactions.Queries.GetFinanceTransactions;

public sealed class GetFinanceTransactionsHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : IQueryHandler<GetFinanceTransactionsQuery, Result<FinanceTransactionPageResponse>>
{
    public async ValueTask<Result<FinanceTransactionPageResponse>> Handle(GetFinanceTransactionsQuery query,
        CancellationToken cancellationToken)
    {
        var names = await FinanceNames.LoadAsync(db, cancellationToken);
        var filtered = Filter(db.Transactions.AsNoTracking(), query.Filter, names);

        var total = await filtered.CountAsync(cancellationToken);
        var sums = await filtered.Where(x => x.Status == MovementStatus.Active)
            .GroupBy(x => new { x.Currency, x.Type })
            .Select(g => new { g.Key.Currency, g.Key.Type, Sum = g.Sum(x => x.AmountMinor) })
            .ToListAsync(cancellationToken);
        IReadOnlyList<FinanceTransactionTotalResponse> totals = sums.GroupBy(x => x.Currency).OrderBy(g => g.Key)
            .Select(g =>
            {
                var income = g.Where(x => x.Type == CategoryDirection.Income).Sum(x => x.Sum);
                var expense = g.Where(x => x.Type == CategoryDirection.Expense).Sum(x => x.Sum);
                return new FinanceTransactionTotalResponse(g.Key, income, expense, income - expense);
            }).ToList();

        var page = await Order(filtered, query.Sort, query.Direction)
            .Skip((query.Page - 1) * FinanceJournal.PageSize).Take(FinanceJournal.PageSize)
            .ToListAsync(cancellationToken);
        names.With(await CounterpartyStates.LoadAsync(new CounterpartyReaders(db, customers, suppliers),
            page.Select(x => x.Counterparty), cancellationToken));
        return Result.Success(new FinanceTransactionPageResponse(
            page.Select(x => FinanceTransactionResponse.From(x, names)).ToList(), total, query.Page,
            FinanceJournal.PageSize, totals));
    }

    private static IQueryable<FinanceTransaction> Filter(IQueryable<FinanceTransaction> rows, FinanceTransactionFilter f,
        FinanceNames names)
    {
        rows = rows.Where(x => x.Scope == f.Scope);
        if (!f.IncludeVoided) rows = rows.Where(x => x.Status == MovementStatus.Active);
        if (f.From is { } from) rows = rows.Where(x => x.Date >= from);
        if (f.To is { } to) rows = rows.Where(x => x.Date <= to);
        if (f.AccountId is { } account)
        {
            var id = FinanceAccountId.From(account);
            rows = rows.Where(x => x.AccountId == id);
        }

        if (f.Type is { } type) rows = rows.Where(x => x.Type == type);
        if (f.SubcategoryId is { } subcategory)
        {
            FinanceCategoryId? id = FinanceCategoryId.From(subcategory);
            rows = rows.Where(x => x.CategoryId == id);
        }
        else if (f.CategoryId is { } category)
        {
            var ids = names.Branch(FinanceCategoryId.From(category));
            rows = rows.Where(x => ids.Contains(x.CategoryId));
        }

        if (f.MinMinor is { } min) rows = rows.Where(x => x.AmountMinor >= min);
        if (f.MaxMinor is { } max) rows = rows.Where(x => x.AmountMinor <= max);
        if (f.Source is { } source) rows = rows.Where(x => x.Source == source);
        if (f.NeedsReview) rows = rows.Where(x => x.Verification != VerificationStatus.Verified);
        if (f.Counterparty is { } link)
            rows = rows.Where(x => x.CounterpartyKind == link.Kind && x.CounterpartyId == link.Id);
        var search = FinanceText.Clean(f.Search).ToLowerInvariant();
        if (search.Length > 0)
            rows = rows.Where(x => x.Description.ToLower().Contains(search) || x.Comment.ToLower().Contains(search)
                || x.OneoffName.ToLower().Contains(search));
        return rows;
    }

    private static IQueryable<FinanceTransaction> Order(IQueryable<FinanceTransaction> rows, FinanceJournalSort sort,
        FinanceSortDirection direction)
    {
        // Ids are time-ordered (UUID v7), so they break ties as «newest saved first».
        var asc = direction == FinanceSortDirection.Asc;
        return sort switch
        {
            FinanceJournalSort.Amount => asc
                ? rows.OrderBy(x => x.AmountMinor).ThenByDescending(x => x.Date).ThenByDescending(x => x.Id)
                : rows.OrderByDescending(x => x.AmountMinor).ThenByDescending(x => x.Date).ThenByDescending(x => x.Id),
            _ => asc
                ? rows.OrderBy(x => x.Date).ThenBy(x => x.Id)
                : rows.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id),
        };
    }
}
