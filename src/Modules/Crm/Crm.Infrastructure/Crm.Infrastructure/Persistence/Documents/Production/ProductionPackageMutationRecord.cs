using Crm.Domain.Quotes.Identifiers;

namespace Crm.Infrastructure.Persistence.Documents.Production;

/// <summary>Append-only export receipt/event; payloads and customer data are never logged here.</summary>
public sealed class ProductionPackageMutationRecord
{
    public long Id { get; set; }
    public int ActorId { get; set; }
    public Guid RequestId { get; set; }
    public QuoteId QuoteId { get; set; }
    public long PackageArtifactId { get; set; }
    public string IntentHash { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
