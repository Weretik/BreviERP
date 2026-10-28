using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
namespace Crm.Application.CustomSewing.GetTemplate.Specifications;

public sealed class TemplateDetailSpec : Specification<CustomTemplateVersion, TemplateDetail>
{
    public TemplateDetailSpec(Guid id, Guid? versionId, CrmActor actor)
    {
        Query.AsNoTracking().Where(x => actor.UserId > 0 && (actor.IsAdmin || x.Template.OwnerId == new CrmUserId(actor.UserId))
            && x.Template.PublicId == id && (versionId.HasValue ? x.PublicId == versionId.Value : x.Number == x.Template.VersionNumber));
        Query.Select(x => new TemplateDetail(x.Template.PublicId, x.Template.OwnerId.Value, x.Template.Revision,
            x.Template.Archived, "customConfiguration", x.PublicId, x.Number, x.Introduction, x.Selections, x.Template.UpdatedAt, !x.Template.Archived));
    }
}
