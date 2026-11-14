using Accounting.Application.Abstractions;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Counterparties.Shared;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Lookup.Shared;
using Accounting.Application.Features.Transactions.Shared;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties.Results;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;

public sealed class GetFinanceCounterpartiesHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
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
                var financialStats = await FinanceCounterpartyChange.OperationStatsAsync(db, CounterpartyKind.Financial,
                    page.Select(x => x.Link.Id).ToList(), cancellationToken);
                financialRows = page.Select(x => FinanceCounterpartyResponse.From(x, names,
                    financialStats.GetValueOrDefault(x.Link.Id))).ToList();
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
        var stats = await FinanceCounterpartyChange.OperationStatsAsync(db, kind,
            rows.Select(x => CounterpartyLink.Of(kind, x.Id).Value.Id).ToList(), cancellationToken);
        return rows.Select(x =>
        {
            var id = CounterpartyLink.Of(kind, x.Id).Value.Id;
            var own = stats.GetValueOrDefault(id);
            return new FinancePartyResponse(kind, id, x.Name, x.TaxId, x.Active, own.Count, own.CurrentTurnoverMinor);
        }).ToList();
    }
}
