using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
namespace Crm.Application.CustomSewing.GetTemplates.Specifications;

public sealed class TemplatePageSpec : Specification<CustomTemplateVersion, TemplateSummary>
{
    public TemplatePageSpec(CrmActor actor, bool includeArchived, int page, int pageSize)
    {
        Query.AsNoTracking().Where(x => actor.UserId > 0 && (actor.IsAdmin || x.Template.OwnerId == new CrmUserId(actor.UserId))
            && (includeArchived || !x.Template.Archived) && x.Number == x.Template.VersionNumber);
        Query.OrderByDescending(x => x.Template.UpdatedAt).ThenBy(x => x.Template.Id).Skip((page - 1) * pageSize).Take(pageSize);
        Query.Select(x => new TemplateSummary(x.Template.PublicId, x.Template.OwnerId.Value, x.Template.Revision,
            x.Template.Archived, "customConfiguration", x.Number, x.Name, x.ProductType, x.Season, x.Template.UpdatedAt));
    }
}
