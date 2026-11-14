namespace Purchasing.Application.Features.Register.Shared;

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
