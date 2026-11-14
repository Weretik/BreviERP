using Identity.Application.Features.Access.Shared;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Access.Queries.GetAccessRights;

public sealed class GetAccessRightsHandler(IAccessRightsService accessRightsService)
    : IQueryHandler<GetAccessRightsQuery, Result<AccessRightsDto>>
{
    public async ValueTask<Result<AccessRightsDto>> Handle(GetAccessRightsQuery query, CancellationToken cancellationToken)
        => Result.Success(await accessRightsService.GetAsync(cancellationToken));
}
