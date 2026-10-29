using Crm.Application.Customers.Access;
using Crm.Application.Documents.Artifacts;
using Crm.Domain.Quotes.Identifiers;
using Crm.Domain.Quotes.Issues;

namespace Crm.Application.Documents.Source;

public interface IQuoteDocumentSource
{
    Task<QuoteIssue?> FindAuthorizedIssueAsync(Guid quotePublicId, Guid versionPublicId,
        CrmActor actor, CancellationToken cancellationToken);
    Task<bool> CanReadQuoteAsync(Guid quotePublicId, QuoteId quoteId, CrmActor actor,
        CancellationToken cancellationToken);
    Task<QuoteDocumentRenderSource?> LoadRenderSourceAsync(QuotePdfArtifactMetadata artifact,
        CancellationToken cancellationToken);
}
