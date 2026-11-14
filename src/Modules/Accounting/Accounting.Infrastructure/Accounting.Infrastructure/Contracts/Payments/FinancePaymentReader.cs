using Accounting.Contracts.Payments;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;

using Mediator;
using Accounting.Application.Features.Payments.Queries.GetFinancePaymentCandidates;
using Accounting.Application.Features.Payments.Queries.GetFinanceTransactionsByIds;
using Accounting.Application.Features.Transactions.Shared;

namespace Accounting.Infrastructure.Contracts.Payments;

internal sealed class FinancePaymentReader(ISender sender) : IFinancePaymentReader
{
    public async Task<IReadOnlyList<FinancePayment>> FindAsync(IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinanceTransactionsByIdsQuery(ids), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Accounting operations could not be read.");
        return result.Value.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<FinancePayment>> CandidatesAsync(int? supplierId, long? amountMinor, string? search,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinancePaymentCandidatesQuery(supplierId, amountMinor, search),
            cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Accounting operations could not be read.");
        return result.Value.Select(Map).ToArray();
    }

    private static FinancePayment Map(FinanceTransactionResponse x) => new(x.Id, x.Date, x.AmountMinor, x.Currency,
        x.AccountName, x.Description, x.Comment, x.Counterparty?.Name, x.OneoffName,
        x.Type == CategoryDirection.Expense, x.Status == MovementStatus.Active);
}
