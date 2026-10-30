using Ardalis.Result;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.Persistence;
using Mediator;

namespace Crm.Application.CustomSewing.GetVersions;

public sealed class GetCustomVersionsQueryHandler(ICustomReadRepository repository)
    : IQueryHandler<GetCustomVersionsQuery, PagedResult<IReadOnlyList<CustomVersionSummary>>>
{
    public async ValueTask<PagedResult<IReadOnlyList<CustomVersionSummary>>> Handle(GetCustomVersionsQuery query, CancellationToken cancellationToken)
    {
        if (query.Page is < 1 or > 1000000 || query.PageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(query));
        return await repository.VersionsAsync(query.DraftId, query.Actor, query.Page, query.PageSize, cancellationToken);
    }
}
