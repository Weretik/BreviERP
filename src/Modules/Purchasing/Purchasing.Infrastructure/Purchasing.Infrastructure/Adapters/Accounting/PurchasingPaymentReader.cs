using Accounting.Contracts.Payments;

using Purchasing.Application.Abstractions;

namespace Purchasing.Infrastructure.Adapters.Accounting;

/// <summary>Expenses of «Бухгалтерія» (Accounting) for the payment links of purchase invoices.</summary>
internal sealed class PurchasingPaymentReader(IFinancePaymentReader payments) : IPurchasingPaymentReader
{
    public async Task<IReadOnlyList<PurchasingPayment>> FindAsync(IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken) =>
        (await payments.FindAsync(ids, cancellationToken)).Select(Map).ToArray();

    public async Task<IReadOnlyList<PurchasingPayment>> CandidatesAsync(int? supplierId, long? amountMinor,
        string? search, CancellationToken cancellationToken) =>
        (await payments.CandidatesAsync(supplierId, amountMinor, search, cancellationToken)).Select(Map).ToArray();

    private static PurchasingPayment Map(FinancePayment x) => new(x.Id, x.Date, x.AmountMinor, x.Currency,
        x.AccountName, string.IsNullOrWhiteSpace(x.Description) ? x.Comment : x.Description,
        x.CounterpartyName ?? (string.IsNullOrWhiteSpace(x.OneoffName) ? null : x.OneoffName),
        x.IsExpense, x.IsActive);
}
