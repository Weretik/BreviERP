using Ardalis.Result;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Mediator;

namespace Crm.Application.CustomSewing.GetTemplates;

public sealed class GetTemplatesQueryHandler(ITemplateReadStore store) : IQueryHandler<GetTemplatesQuery, PagedResult<IReadOnlyList<TemplateSummary>>>
{
    public async ValueTask<PagedResult<IReadOnlyList<TemplateSummary>>> Handle(GetTemplatesQuery query, CancellationToken ct)
    {
        if (query.Page is < 1 or > 1000000 || query.PageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(query));
        return await store.ListAsync(query.Actor, query.IncludeArchived, query.Page, query.PageSize, ct);
    }
}
