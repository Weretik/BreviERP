using Accounting.Domain.Counterparties;
using BuildingBlocks.Domain.Results;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Api.Counterparties;

/// <summary>The counterparty filter of a list or a report: a kind and an id, or nothing when either is missing.</summary>
internal static class AccountingQueryLink
{
    public static DomainResult<CounterpartyLink?> Of(CounterpartyKind? kind, string? id) =>
        FinanceCounterpartyLinkInput.LinkOf(kind is { } known && !string.IsNullOrWhiteSpace(id)
            ? new FinanceCounterpartyLinkInput(known, id)
            : null);
}
