using Ardalis.Result;
using Crm.Application.Documents.Contracts;
using Crm.Application.Documents.Persistence;
using Mediator;

namespace Crm.Application.Documents.GetIssuerSettings;

public sealed class GetIssuerSettingsQueryHandler(IDocumentSettingsStore store)
    : IQueryHandler<GetIssuerSettingsQuery, Result<ActiveIssuerSettings>>
{
    public async ValueTask<Result<ActiveIssuerSettings>> Handle(GetIssuerSettingsQuery query, CancellationToken ct)
        => Result.Success(await store.GetActiveAsync(ct));
}
