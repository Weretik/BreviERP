using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Payments.Queries.GetFinancePaymentCandidates;

/// <summary>
/// Expenses that may pay a purchase invoice: active, of both scopes. Without <c>Search</c> — the expenses of
/// <c>SupplierId</c>; with it — any expense whose description, note or one-off name contains it, or whose amount equals it
/// in hryvnias. The expenses with <c>AmountMinor</c> come first, then the newest; at most <see cref="Limit"/>.
/// </summary>
public sealed record GetFinancePaymentCandidatesQuery(int? SupplierId, long? AmountMinor, string? Search)
    : IQuery<Result<IReadOnlyList<FinanceTransactionResponse>>>
{
    public const int Limit = 20;
    public const int SearchMaxLength = 200;
}
