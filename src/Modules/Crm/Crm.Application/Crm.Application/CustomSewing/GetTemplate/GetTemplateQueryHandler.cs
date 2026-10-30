using Ardalis.Result;
using Crm.Application.CustomSewing.Templates.Contracts;
using Crm.Application.CustomSewing.Templates.Persistence;
using Mediator;

namespace Crm.Application.CustomSewing.GetTemplate;

public sealed class GetTemplateQueryHandler(ITemplateReadStore store) : IQueryHandler<GetTemplateQuery, Result<TemplateDetail>>
{
    public async ValueTask<Result<TemplateDetail>> Handle(GetTemplateQuery query, CancellationToken ct)
    {
        var result = await store.DetailAsync(query.TemplateId, query.VersionId, query.Actor, ct);
        return result is null ? Result.NotFound() : Result.Success(result);
    }
}
