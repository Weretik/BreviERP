using Ardalis.Result;
using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Domain.CustomSewing.Drafts;

namespace Crm.Application.CustomSewing.Persistence;

public interface ICustomReadRepository : IReadRepositoryBase<CustomSewingDraft>
{
    Task<CustomVersionView?> VersionAsync(Guid draftId, Guid versionId, CrmActor actor, CancellationToken cancellationToken);
    Task<PagedResult<IReadOnlyList<CustomVersionSummary>>> VersionsAsync(Guid draftId, CrmActor actor,
        int page, int pageSize, CancellationToken cancellationToken);
}
