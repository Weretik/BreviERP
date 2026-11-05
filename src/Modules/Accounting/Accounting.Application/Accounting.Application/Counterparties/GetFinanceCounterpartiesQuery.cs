using Accounting.Application.Abstractions;
using Accounting.Application.Contracts;
using Accounting.Application.Journal;
using Accounting.Application.Lookup;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;

namespace Accounting.Application.Counterparties;

/// <summary>
/// «Контрагенти»: one tab, 50 rows per page, with the counts of every tab for the same search. Without
/// <c>IncludeArchived</c> archived counterparties, inactive clients and suppliers, and cancelled one-off operations are
/// left out.
/// </summary>
public sealed record GetFinanceCounterpartiesQuery(FinanceCounterpartyTab Tab, string? Search, bool IncludeArchived, int Page)
    : IQuery<Result<FinanceCounterpartyPageResponse>>;

public sealed class GetFinanceCounterpartiesQueryValidator : AbstractValidator<GetFinanceCounterpartiesQuery>
{
    public GetFinanceCounterpartiesQueryValidator()
    {
        RuleFor(x => x.Tab).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(FinanceJournal.SearchMaxLength);
        RuleFor(x => x.Page).InclusiveBetween(1, 100_000);
    }
}

public sealed class GetFinanceCounterpartiesQueryHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers) : IQueryHandler<GetFinanceCounterpartiesQuery, Result<FinanceCounterpartyPageResponse>>
{
    private const int PageSize = FinanceJournal.PageSize;

    public async ValueTask<Result<FinanceCounterpartyPageResponse>> Handle(GetFinanceCounterpartiesQuery query,
        CancellationToken cancellationToken)
    {
        var search = FinanceText.Name(query.Search);
        var term = search.ToLowerInvariant();
        var skip = (query.Page - 1) * PageSize;
        var names = await FinanceNames.LoadAsync(db, cancellationToken);

        var financial = Financial(term, query.IncludeArchived);
        var oneoffs = Oneoffs(term, query.IncludeArchived);
        var supplierRows = Matching(await suppliers.ListAsync(cancellationToken), term, query.IncludeArchived);
        var clientPage = await customers.SearchAsync(search.Length > 0 ? search : null, !query.IncludeArchived,
            query.Tab == FinanceCounterpartyTab.Clients ? query.Page : 1,
            query.Tab == FinanceCounterpartyTab.Clients ? PageSize : 1, cancellationToken);
        var counts = new FinanceCounterpartyCountsResponse(await financial.CountAsync(cancellationToken), clientPage.Total,
            supplierRows.Count, await oneoffs.CountAsync(cancellationToken));

        IReadOnlyList<FinanceCounterpartyResponse> financialRows = [];
        IReadOnlyList<FinancePartyResponse> parties = [];
        IReadOnlyList<FinanceTransactionResponse> oneoffRows = [];
        int total;
        switch (query.Tab)
        {
            case FinanceCounterpartyTab.Financial:
                var page = await financial.OrderBy(x => x.Name).ThenBy(x => x.Id).Skip(skip).Take(PageSize)
                    .ToListAsync(cancellationToken);
                var financialCounts = await FinanceCounterpartyChange.OperationCountsAsync(db, CounterpartyKind.Financial,
                    page.Select(x => x.Link.Id).ToList(), cancellationToken);
                financialRows = page.Select(x => FinanceCounterpartyResponse.From(x, names,
                    financialCounts.GetValueOrDefault(x.Link.Id))).ToList();
                total = counts.Financial;
                break;
            case FinanceCounterpartyTab.Clients:
                parties = await PartiesAsync(CounterpartyKind.Client, clientPage.Items, cancellationToken);
                total = counts.Clients;
                break;
            case FinanceCounterpartyTab.Suppliers:
                parties = await PartiesAsync(CounterpartyKind.Supplier,
                    supplierRows.OrderBy(x => x.Name).Skip(skip).Take(PageSize).ToList(), cancellationToken);
                total = counts.Suppliers;
                break;
            default:
                var operations = await oneoffs.OrderByDescending(x => x.Date).ThenByDescending(x => x.Id).Skip(skip)
                    .Take(PageSize).ToListAsync(cancellationToken);
                oneoffRows = operations.Select(x => FinanceTransactionResponse.From(x, names)).ToList();
                total = counts.Oneoffs;
                break;
        }

        return Result.Success(new FinanceCounterpartyPageResponse(query.Tab, financialRows, parties, oneoffRows, total,
            query.Page, PageSize, counts));
    }

    private IQueryable<FinanceCounterparty> Financial(string term, bool includeArchived)
    {
        var rows = db.Counterparties.AsNoTracking();
        if (!includeArchived) rows = rows.Where(x => x.Active);
        return term.Length > 0 ? rows.Where(x => x.SearchText.Contains(term)) : rows;
    }

    private IQueryable<FinanceTransaction> Oneoffs(string term, bool includeArchived)
    {
        var rows = db.Transactions.AsNoTracking().Where(x => x.OneoffName != "");
        if (!includeArchived) rows = rows.Where(x => x.Status == MovementStatus.Active);
        return term.Length > 0 ? rows.Where(x => x.OneoffName.ToLower().Contains(term)) : rows;
    }

    internal static List<AccountingParty> Matching(IEnumerable<AccountingParty> parties, string term, bool includeArchived) =>
        parties.Where(x => includeArchived || x.Active)
            .Where(x => term.Length == 0 || x.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || x.TaxId.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

    private async Task<IReadOnlyList<FinancePartyResponse>> PartiesAsync(CounterpartyKind kind,
        IReadOnlyList<AccountingParty> rows, CancellationToken cancellationToken)
    {
        var counts = await FinanceCounterpartyChange.OperationCountsAsync(db, kind,
            rows.Select(x => CounterpartyLink.Of(kind, x.Id).Id).ToList(), cancellationToken);
        return rows.Select(x =>
        {
            var id = CounterpartyLink.Of(kind, x.Id).Id;
            return new FinancePartyResponse(kind, id, x.Name, x.TaxId, x.Active, counts.GetValueOrDefault(id));
        }).ToList();
    }
}
