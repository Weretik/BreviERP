using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Documents.Contracts;

namespace Crm.Application.Documents.Persistence;

public interface IDocumentSettingsStore
{
    Task<ActiveIssuerSettings> GetActiveAsync(CancellationToken ct);
    Task<Result<IssuerSettingsDetail>> CreateVersionAsync(Guid requestId, IssuerSettingsInput issuer,
        DocumentTemplateInput template, CrmActor actor, CancellationToken ct);
    Task<Result<ActiveIssuerSettings>> ActivateAsync(Guid requestId, long expectedRevision,
        Guid issuerVersionId, Guid templateVersionId, CrmActor actor, CancellationToken ct);
}
