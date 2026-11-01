using Crm.Domain.Delivery;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed class QuoteDeliveryResolution
{
    public QuoteDeliveryId DeliveryId { get; init; }
    public QuoteId QuoteId { get; init; }
    public int ActorId { get; init; }
    public int Attempt { get; init; }
    public string Outcome { get; init; } = null!;
    public string EvidenceNote { get; init; } = null!;
    public DateTimeOffset CreatedAt { get; init; }
}
