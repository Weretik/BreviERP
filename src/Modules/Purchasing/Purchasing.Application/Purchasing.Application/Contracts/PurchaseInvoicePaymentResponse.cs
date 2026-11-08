namespace Purchasing.Application.Contracts;

/// <summary>
/// A linked payment of an invoice: the expense of «Бухгалтерія» as it is now. <c>Missing</c> — the operation is not found;
/// <c>Voided</c> — it was voided after linking. Amounts are kopecks.
/// </summary>
public sealed record PurchaseInvoicePaymentResponse(
    Guid TransactionId,
    DateOnly? Date,
    long? AmountMinor,
    string? Currency,
    string? AccountName,
    string? Description,
    string? CounterpartyName,
    bool Missing,
    bool Voided,
    string LinkedBy,
    DateTimeOffset LinkedAt);

/// <summary>An expense offered for «Прив'язати оплату»; <c>SameAmount</c> — it equals the invoice total.</summary>
public sealed record PurchasePaymentCandidateResponse(
    Guid TransactionId,
    DateOnly Date,
    long AmountMinor,
    string Currency,
    string AccountName,
    string Description,
    string? CounterpartyName,
    bool SameAmount);

/// <summary>The payments of one invoice after a change, with the new review version.</summary>
public sealed record PurchaseInvoicePaymentsResponse(
    string MessageId,
    int ReviewVersion,
    long? TotalPayableMinor,
    IReadOnlyList<PurchaseInvoicePaymentResponse> Payments,
    IReadOnlyList<string> PaymentIssues);
