using Ardalis.Result;
using Crm.Application.Documents.Artifacts;
using Crm.Application.Documents.Source;
using Mediator;

namespace Crm.Application.Documents.GetQuoteArtifact;

public sealed class GetQuoteArtifactQueryHandler(IQuotePdfArtifactStore artifacts, IQuoteDocumentSource source)
    : IQueryHandler<GetQuoteArtifactQuery, Result<QuotePdfArtifactDetail>>
{
    public async ValueTask<Result<QuotePdfArtifactDetail>> Handle(GetQuoteArtifactQuery query,
        CancellationToken cancellationToken)
    {
        if (query.QuoteId == Guid.Empty || query.ArtifactId == Guid.Empty || query.Actor.UserId <= 0)
            return Result.NotFound();
        var metadata = await artifacts.FindAsync(query.ArtifactId, cancellationToken);
        if (metadata is null || !await source.CanReadQuoteAsync(query.QuoteId, metadata.QuoteId,
            query.Actor, cancellationToken)) return Result.NotFound();
        return Result.Success(QuotePdfArtifactDetail.From(metadata));
    }
}
