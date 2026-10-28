namespace Crm.Infrastructure.Persistence.Templates.Writing;

public sealed class TemplateMutationRecord
{
    public long Id { get; set; }
    public int ActorId { get; set; }
    public string Operation { get; set; } = null!;
    public Guid RequestId { get; set; }
    public Guid TemplatePublicId { get; set; }
    public string IntentHash { get; set; } = null!;
    public string ResponseJson { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
