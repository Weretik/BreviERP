namespace Crm.Application.Delivery.ResolveOutcome;

public sealed record ResolveQuoteDeliveryCommand(Guid QuoteId, Guid DeliveryId, Guid RequestId,
    int ExpectedAttempt, string Outcome, string EvidenceNote);
