using Accounting.Domain.Counterparties;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Api.Endpoints.Shared;

/// <summary>The counterparty filter of a list or a report: a kind and an id, or nothing when either is missing.</summary>
internal static class AccountingQueryLink
{
    public static FinanceCounterpartyLinkInput? Of(CounterpartyKind? kind, string? id) =>
        kind is { } known && !string.IsNullOrWhiteSpace(id) ? new FinanceCounterpartyLinkInput(known, id) : null;
}
