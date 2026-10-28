using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Crm.Application.Customers.Access;
using Crm.Application.CustomSewing.GetTemplate.Specifications;
using Crm.Application.CustomSewing.GetTemplates.Specifications;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Crm.Domain.CustomSewing.Templates;
using Microsoft.EntityFrameworkCore;
namespace Crm.Infrastructure.Persistence.Templates.Reading;

public sealed class TemplateReadStore(CrmDbContext db) : ITemplateReadStore
{
    public Task<TemplateDetail?> DetailAsync(Guid id, Guid? versionId, CrmActor actor, CancellationToken ct)
        => SpecificationEvaluator.Default.GetQuery(db.Set<CustomTemplateVersion>(), new TemplateDetailSpec(id, versionId, actor)).SingleOrDefaultAsync(ct);
    public async Task<PagedResult<IReadOnlyList<TemplateSummary>>> ListAsync(CrmActor actor, bool includeArchived, int page, int pageSize, CancellationToken ct)
    {
        var count = await SpecificationEvaluator.Default.GetQuery(db.Set<CustomTemplateVersion>(), new TemplateCountSpec(actor, includeArchived)).CountAsync(ct);
        var rows = await SpecificationEvaluator.Default.GetQuery(db.Set<CustomTemplateVersion>(), new TemplatePageSpec(actor, includeArchived, page, pageSize)).ToArrayAsync(ct);
        return new(new PagedInfo(page, pageSize, (long)Math.Ceiling(count / (double)pageSize), count), rows);
    }
}
