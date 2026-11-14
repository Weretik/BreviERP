namespace Accounting.Contracts.Payments;

/// <summary>Operations of «Бухгалтерія» that other modules link as payments (purchase invoices).</summary>
public interface IFinancePaymentReader
{
    /// <summary>Operations by id, voided ones too; unknown ids are left out.</summary>
    Task<IReadOnlyList<FinancePayment>> FindAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);

    /// <summary>
    /// Active expenses that may pay an invoice: of the supplier, or matching the search; the ones with the amount come first.
    /// </summary>
    Task<IReadOnlyList<FinancePayment>> CandidatesAsync(int? supplierId, long? amountMinor, string? search,
        CancellationToken cancellationToken);
}

/// <summary>One operation; <paramref name="AmountMinor"/> is positive kopecks.</summary>
public sealed record FinancePayment(
    Guid Id,
    DateOnly Date,
    long AmountMinor,
    string Currency,
    string AccountName,
    string Description,
    string Comment,
    string? CounterpartyName,
    string OneoffName,
    bool IsExpense,
    bool IsActive);
