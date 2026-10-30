namespace Crm.Infrastructure.Persistence.CustomSewing.Pricing;

public sealed class CustomCalculationRecord
{
    public long Id { get; set; }
    public Guid Token { get; set; }
    public Crm.Domain.CustomSewing.Identifiers.CustomDraftId DraftId { get; set; }
    public long Revision { get; set; }
    public int ActorId { get; set; }
    public string BasisJson { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
}
