using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Application.Documents.Artifacts;

public sealed record QuotePdfArtifactMetadata(
    Guid PublicId, QuoteId QuoteId, long QuoteVersionId,
    IssuerSettingsVersionId IssuerVersionId, DocumentTemplateVersionId TemplateVersionId,
    string RenderKey, string Status, int Attempt, string? Sha256,
    long? Length, string? ErrorCode, DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt, DateTimeOffset? GeneratedAt);
