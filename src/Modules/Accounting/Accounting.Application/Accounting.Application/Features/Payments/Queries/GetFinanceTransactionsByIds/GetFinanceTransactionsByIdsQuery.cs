using Accounting.Application.Abstractions;
using Accounting.Domain.Categories;
using Accounting.Domain.Counterparties;
using Accounting.Domain.Ledger;
using Accounting.Domain.Movements;
using Accounting.Domain.Transactions;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Application.Features.Payments.Queries.GetFinanceTransactionsByIds;

/// <summary>
/// Operations by id for the payment links of other modules («Рахунки на закупку»): voided ones too, so a link shows
/// what happened; unknown ids are left out.
/// </summary>
public sealed record GetFinanceTransactionsByIdsQuery(IReadOnlyCollection<Guid> Ids)
    : IQuery<Result<IReadOnlyList<FinanceTransactionResponse>>>;
