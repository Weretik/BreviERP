namespace Crm.Application.Delivery.Provider;

public sealed record QuoteDeliveryProviderResult(QuoteDeliveryProviderOutcome Outcome,
    string? ProviderMessageId, string? ErrorCode);
