namespace Crm.Application.Delivery.Readiness;

/// <summary>Safe prerequisite result containing bounded codes, never addresses or credentials.</summary>
public sealed record QuoteDeliveryReadiness(bool CanSubmit, IReadOnlyList<string> Blockers);
