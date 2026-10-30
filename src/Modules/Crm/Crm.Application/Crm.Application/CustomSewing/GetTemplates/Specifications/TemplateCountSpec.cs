using Ardalis.Specification;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Domain.Customers.Identifiers;
using Crm.Domain.CustomSewing.Templates;
namespace Crm.Application.CustomSewing.GetTemplates.Specifications;

public sealed class TemplateCountSpec : Specification<CustomTemplateVersion>
{
    public TemplateCountSpec(CrmActor actor, bool includeArchived)
    {
        Query.AsNoTracking().Where(x => actor.UserId > 0 && (actor.IsAdmin || x.Template.OwnerId == new CrmUserId(actor.UserId))
            && (includeArchived || !x.Template.Archived) && x.Number == x.Template.VersionNumber);

    }
}
