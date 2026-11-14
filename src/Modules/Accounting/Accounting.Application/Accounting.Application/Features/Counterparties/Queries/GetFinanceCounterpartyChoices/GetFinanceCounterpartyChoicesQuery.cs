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
