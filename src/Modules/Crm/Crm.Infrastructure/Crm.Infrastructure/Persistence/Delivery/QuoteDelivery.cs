using Crm.Domain.Delivery;
using Crm.Domain.Documents.EmailTemplates;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Infrastructure.Persistence.Delivery;

/// <summary>Immutable private message pins. Mutable leases/outcomes live in QuoteDeliveryState.</summary>
public sealed class QuoteDelivery
{
    public QuoteDeliveryId Id { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public QuoteId QuoteId { get; set; }
    public long QuoteVersionId { get; set; }
    public long ArtifactId { get; set; }
    public EmailTemplateVersionId EmailTemplateVersionId { get; set; }
    public int ActorId { get; set; }
    public string ProviderKey { get; set; } = null!;
    public string SenderAddress { get; set; } = null!;
    public string RecipientAddress { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string PlainText { get; set; } = null!;
    public byte[] PdfBytes { get; set; } = null!;
    public string PdfSha256 { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
}
