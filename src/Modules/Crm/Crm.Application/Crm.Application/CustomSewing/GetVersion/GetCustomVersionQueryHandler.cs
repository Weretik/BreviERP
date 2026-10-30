using Ardalis.Result;
using Crm.Application.CustomSewing.Contracts;
using Crm.Application.CustomSewing.Persistence;
using Mediator;

namespace Crm.Application.CustomSewing.GetVersion;

public sealed class GetCustomVersionQueryHandler(ICustomReadRepository repository)
    : IQueryHandler<GetCustomVersionQuery, Result<CustomVersionView>>
{
    public async ValueTask<Result<CustomVersionView>> Handle(GetCustomVersionQuery query, CancellationToken cancellationToken)
    {
        var version = await repository.VersionAsync(query.DraftId, query.VersionId, query.Actor, cancellationToken);
        return version is null ? Result.NotFound() : Result.Success(version);
    }
}
