using Accounting.Application.Contracts;
using Accounting.Application.Payments;
using Accounting.Domain.Categories;
using Accounting.Domain.Movements;
using Mediator;
using Purchasing.Application.Abstractions;

namespace Host.Api.Purchasing;

/// <summary>Expenses of «Бухгалтерія» (Accounting) for the payment links of purchase invoices.</summary>
internal sealed class PurchasingPaymentReader(ISender sender) : IPurchasingPaymentReader
{
    public async Task<IReadOnlyList<PurchasingPayment>> FindAsync(IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinanceTransactionsByIdsQuery(ids), cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Accounting operations could not be read.");
        return result.Value.Select(Map).ToArray();
    }

    public async Task<IReadOnlyList<PurchasingPayment>> CandidatesAsync(int? supplierId, long? amountMinor, string? search,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFinancePaymentCandidatesQuery(supplierId, amountMinor, search),
            cancellationToken);
        if (!result.IsSuccess) throw new InvalidOperationException("Accounting operations could not be read.");
        return result.Value.Select(Map).ToArray();
    }

    private static PurchasingPayment Map(FinanceTransactionResponse x) => new(x.Id, x.Date, x.AmountMinor, x.Currency,
        x.AccountName, string.IsNullOrWhiteSpace(x.Description) ? x.Comment : x.Description,
        x.Counterparty?.Name ?? (string.IsNullOrWhiteSpace(x.OneoffName) ? null : x.OneoffName),
        x.Type == CategoryDirection.Expense, x.Status == MovementStatus.Active);
}
