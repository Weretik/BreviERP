namespace Purchasing.Application.Abstractions;

/// <summary>Expenses of «Бухгалтерія» (Accounting) for the payment links of purchase invoices; implemented in the host.</summary>
public interface IPurchasingPaymentReader
{
    /// <summary>Operations by id, voided ones too; unknown ids are left out.</summary>
    Task<IReadOnlyList<PurchasingPayment>> FindAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Active expenses that may pay an invoice: by <paramref name="search"/> (text or amount) when given, else of
    /// <paramref name="supplierId"/>, else with exactly <paramref name="amountMinor"/>; that amount first, then the newest.
    /// </summary>
    Task<IReadOnlyList<PurchasingPayment>> CandidatesAsync(int? supplierId, long? amountMinor, string? search,
        CancellationToken cancellationToken);
}

/// <summary>An operation of «Бухгалтерія» as a payment: amount in kopecks; <c>Expense</c> and <c>Active</c> as it is now.</summary>
public sealed record PurchasingPayment(
    Guid Id,
    DateOnly Date,
    long AmountMinor,
    string Currency,
    string AccountName,
    string Description,
    string? CounterpartyName,
    bool Expense,
    bool Active);
