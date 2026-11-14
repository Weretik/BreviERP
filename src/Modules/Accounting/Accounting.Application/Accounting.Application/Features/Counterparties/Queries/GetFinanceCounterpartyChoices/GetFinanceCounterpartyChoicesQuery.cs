using Accounting.Application.Abstractions;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterparties;
using Accounting.Application.Features.Journal.Shared;

namespace Accounting.Application.Features.Counterparties.Queries.GetFinanceCounterpartyChoices;

/// <summary>
/// Choices of the counterparty field of an operation: active finance counterparties, clients and suppliers whose name
/// or code matches <c>Search</c>, at most <see cref="PerKind"/> of each kind in that order.
/// </summary>
public sealed record GetFinanceCounterpartyChoicesQuery(string? Search)
    : IQuery<Result<IReadOnlyList<FinanceCounterpartyChoiceResponse>>>
{
    public const int PerKind = 15;
}

public sealed class GetFinanceCounterpartyChoicesValidator : AbstractValidator<GetFinanceCounterpartyChoicesQuery>
{
    public GetFinanceCounterpartyChoicesValidator() =>
        RuleFor(x => x.Search).MaximumLength(FinanceJournal.SearchMaxLength);
}

public sealed class GetFinanceCounterpartyChoicesHandler(IAccountingDbContext db, IAccountingCustomerReader customers,
    IAccountingSupplierReader suppliers)
    : IQueryHandler<GetFinanceCounterpartyChoicesQuery, Result<IReadOnlyList<FinanceCounterpartyChoiceResponse>>>
{
    private const int PerKind = GetFinanceCounterpartyChoicesQuery.PerKind;

    public async ValueTask<Result<IReadOnlyList<FinanceCounterpartyChoiceResponse>>> Handle(
        GetFinanceCounterpartyChoicesQuery query, CancellationToken cancellationToken)
    {
        var search = FinanceText.Name(query.Search);
        var term = search.ToLowerInvariant();
        var financialRows = db.Counterparties.AsNoTracking().Where(x => x.Active);
        if (term.Length > 0) financialRows = financialRows.Where(x => x.SearchText.Contains(term));
        var financial = await financialRows.OrderBy(x => x.Name).ThenBy(x => x.Id).Take(PerKind)
            .ToListAsync(cancellationToken);
        var clients = await customers.SearchAsync(search.Length > 0 ? search : null, true, 1, PerKind, cancellationToken);
        var supplierRows = GetFinanceCounterpartiesHandler.Matching(await suppliers.ListAsync(cancellationToken), term,
            false).OrderBy(x => x.Name).Take(PerKind);

        IReadOnlyList<FinanceCounterpartyChoiceResponse> choices = financial
            .Select(x => new FinanceCounterpartyChoiceResponse(CounterpartyKind.Financial, x.Link.Id, x.Name, x.Edrpou,
                x.Verified, x.DefaultCategoryId?.Value))
            .Concat(clients.Items.Select(x => Party(CounterpartyKind.Client, x)))
            .Concat(supplierRows.Select(x => Party(CounterpartyKind.Supplier, x)))
            .ToList();
        return Result.Success(choices);
    }

    private static FinanceCounterpartyChoiceResponse Party(CounterpartyKind kind, AccountingParty party) =>
        new(kind, CounterpartyLink.Of(kind, party.Id).Value.Id, party.Name, party.TaxId, true, null);
}
