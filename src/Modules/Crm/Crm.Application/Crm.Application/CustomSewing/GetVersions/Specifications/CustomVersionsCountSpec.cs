using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Domain.CustomSewing.Versions;

namespace Crm.Application.CustomSewing.GetVersions.Specifications;

public sealed class CustomVersionsCountSpec : Specification<CustomSewingVersion>
{
    public CustomVersionsCountSpec(Guid draftId, CrmActor actor)
        => Query.AsNoTracking().Where(CustomAccess.ForVersion(actor)).Where(x => x.Draft.PublicId == draftId);
}
