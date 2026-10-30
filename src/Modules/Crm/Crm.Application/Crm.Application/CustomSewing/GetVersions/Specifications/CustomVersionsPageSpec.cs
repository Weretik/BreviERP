using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Domain.CustomSewing.Versions;

namespace Crm.Application.CustomSewing.GetVersions.Specifications;

public sealed class CustomVersionsPageSpec : Specification<CustomSewingVersion, CustomVersionSummary>
{
    public CustomVersionsPageSpec(Guid draftId, CrmActor actor, int page, int pageSize)
    {
        Query.AsNoTracking().Where(CustomAccess.ForVersion(actor)).Where(x => x.Draft.PublicId == draftId)
            .OrderByDescending(x => x.Number).ThenByDescending(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize);
        Query.Select(x => new CustomVersionSummary(x.PublicId, x.Number,
            x.ParentVersion == null ? null : x.ParentVersion.PublicId, x.Name, x.ActorId.Value, x.CreatedAt));
    }
}
