using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;

namespace Crm.Application.CustomSewing.Templates.Persistence;

public interface ITemplateReadStore
{
    Task<TemplateDetail?> DetailAsync(Guid id, Guid? versionId, CrmActor actor, CancellationToken ct);
    Task<PagedResult<IReadOnlyList<TemplateSummary>>> ListAsync(CrmActor actor, bool includeArchived, int page, int pageSize, CancellationToken ct);
}
