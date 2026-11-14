namespace Purchasing.Application.Features.Register.Queries.GetPurchasePaymentCandidates;

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
