using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Access;
using Crm.Application.CustomSewing.Contracts;
using Crm.Domain.CustomSewing.Drafts;
using Crm.Domain.Customers;

namespace Crm.Application.CustomSewing.GetDraft.Specifications;

public sealed class CustomDraftDetailSpec : Specification<CustomSewingDraft, CustomDetail>
{
    public CustomDraftDetailSpec(Guid publicId, CrmActor actor)
    {
        Query.AsNoTracking().Where(CustomAccess.For(actor)).Where(x => x.PublicId == publicId);
        Query.Select(Crm.Application.CustomSewing.Mapping.CustomDetailProjection.Value);
    }
}
