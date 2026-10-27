namespace Crm.Infrastructure.Persistence.Quotes.Pricing;

public sealed class QuotePriceBasisRecord
{
    public long Id { get; set; }
    public Guid PublicId { get; set; }
    public Guid RequestId { get; set; }
    public int ActorId { get; set; }
    public Guid? QuotePublicId { get; set; }
    public string IntentHash { get; set; } = null!;
    public string SnapshotJson { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}
