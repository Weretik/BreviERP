using Accounting.Domain.Categories;
using Accounting.Domain.Ledger;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Api.Endpoints.Transactions.ReviewFinanceTransaction;

/// <summary>
/// «Перевірити»: the category and who the operation is with, with the <c>Version</c> the user opened.
/// <c>RememberCategory</c> («Запамʼятати») makes the category the default of a finance counterparty.
/// </summary>
public sealed record FinanceTransactionReviewInput(Guid CategoryId, FinanceCounterpartyLinkInput? Counterparty,
    string? OneoffName, int Version, bool RememberCategory = false);
