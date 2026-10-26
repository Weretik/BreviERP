namespace Crm.Infrastructure.Persistence;

public sealed class CustomerMutationRecord
{
    public long Id { get; set; }
    public int ActorId { get; set; }
    public string Operation { get; set; } = null!;
    public Guid RequestId { get; set; }
    public string PayloadHash { get; set; } = null!;
    public Guid CustomerPublicId { get; set; }
    public string IntentJson { get; set; } = null!;
    public string ResponseJson { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
