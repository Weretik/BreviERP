namespace Crm.Application.Delivery.Outbox;

public sealed record QuoteDeliveryManualResolution(string Outcome, int ActorId, int Attempt,
    string EvidenceNote, DateTimeOffset CreatedAt);
