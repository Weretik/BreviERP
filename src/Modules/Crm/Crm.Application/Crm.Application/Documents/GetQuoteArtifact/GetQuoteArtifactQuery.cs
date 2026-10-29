using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Artifacts;
using Mediator;

namespace Crm.Application.Documents.GetQuoteArtifact;

public sealed record GetQuoteArtifactQuery(Guid QuoteId, Guid ArtifactId, CrmActor Actor)
    : IQuery<Result<QuotePdfArtifactDetail>>;
