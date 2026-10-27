namespace Crm.Infrastructure.Persistence.Quotes.Writing;

public sealed class QuoteMutationRecord
{
    public long Id { get; set; }
    public int ActorId { get; set; }
    public string Operation { get; set; } = null!;
    public Guid RequestId { get; set; }
    public string IntentHash { get; set; } = null!;
    public Guid QuotePublicId { get; set; }
    public string ResponseJson { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
