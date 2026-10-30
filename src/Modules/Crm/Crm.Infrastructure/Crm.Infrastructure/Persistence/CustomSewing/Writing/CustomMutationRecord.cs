namespace Crm.Infrastructure.Persistence.CustomSewing.Writing;

public sealed class CustomMutationRecord
{
    public long Id { get; set; }
    public Guid RequestId { get; set; }
    public int ActorId { get; set; }
    public string Operation { get; set; } = null!;
    public string IntentHash { get; set; } = null!;
    public Guid DraftPublicId { get; set; }
    public string ResponseJson { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
