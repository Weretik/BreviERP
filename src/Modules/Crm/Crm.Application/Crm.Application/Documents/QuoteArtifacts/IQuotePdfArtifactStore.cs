using Ardalis.Result;
using Crm.Domain.Documents.Issuer;
using Crm.Domain.Documents.Templates;
using Crm.Domain.Quotes.Identifiers;

namespace Crm.Application.Documents.Artifacts;

// The caller must authorize quote access and verify the pinned QuoteIssue before using this store.
public interface IQuotePdfArtifactStore
{
    Task<Result<QuotePdfArtifactMetadata>> QueueAsync(QuoteId quoteId, long quoteVersionId,
        IssuerSettingsVersionId issuerVersionId, DocumentTemplateVersionId templateVersionId,
        string renderKey, CancellationToken cancellationToken);
    Task<QuotePdfArtifactClaim?> ClaimNextAsync(CancellationToken cancellationToken);
    Task<Result<QuotePdfArtifactMetadata>> CompleteAsync(Guid artifactId, Guid leaseToken,
        byte[] pdfBytes, CancellationToken cancellationToken);
    Task<Result<QuotePdfArtifactMetadata>> FailAsync(Guid artifactId, Guid leaseToken,
        string errorCode, CancellationToken cancellationToken);
    Task<Result<QuotePdfArtifactMetadata>> RetryAsync(Guid artifactId, CancellationToken cancellationToken);
    Task<QuotePdfArtifactMetadata?> FindAsync(Guid artifactId, CancellationToken cancellationToken);
    Task<Result<QuotePdfArtifactContent>> ReadContentAsync(Guid artifactId, CancellationToken cancellationToken);
}
