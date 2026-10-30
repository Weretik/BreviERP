using Crm.Domain.Delivery;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Infrastructure.Persistence.Delivery;

public sealed class QuoteDeliveryEvent
{
    public long Id { get; set; }
    public QuoteDeliveryId DeliveryId { get; set; }
    public QuoteId QuoteId { get; set; }
    public long QuoteVersionId { get; set; }
    public int ActorId { get; set; }
    public int Attempt { get; set; }
    public string Kind { get; set; } = null!;
    public string? ErrorCode { get; set; }
    public string? ProviderMessageId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
