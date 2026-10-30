using Crm.Domain.Delivery;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed class QuoteDeliveryRequest
{
    public long Id { get; set; }
    public int ActorId { get; set; }
    public Guid RequestId { get; set; }
    public QuoteId QuoteId { get; set; }
    public QuoteDeliveryId DeliveryId { get; set; }
    public string IntentHash { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
