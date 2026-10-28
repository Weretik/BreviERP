using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Domain.CustomSewing.Versions;

namespace Crm.Application.CustomSewing.GetVersion.Specifications;

public sealed class CustomVersionDetailSpec : Specification<CustomSewingVersion, CustomVersionView>
{
    public CustomVersionDetailSpec(Guid draftId, Guid versionId, CrmActor actor)
    {
        Query.AsNoTracking().Where(CustomAccess.ForVersion(actor)).Where(x => x.Draft.PublicId == draftId && x.PublicId == versionId);
        Query.Select(x => new CustomVersionView(x.PublicId, x.Number,
            x.ParentVersion == null ? null : x.ParentVersion.PublicId, x.Configuration, x.Basis, x.ActorId.Value, x.CreatedAt));
    }
}
