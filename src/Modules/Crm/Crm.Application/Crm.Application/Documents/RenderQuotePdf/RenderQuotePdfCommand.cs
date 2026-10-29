using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Artifacts;
using Mediator;

namespace Crm.Application.Documents.RenderQuotePdf;

public sealed record RenderQuotePdfCommand(Guid QuoteId, Guid VersionId, CrmActor Actor)
    : ICommand<Result<QuotePdfArtifactDetail>>;
