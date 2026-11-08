using Identity.Application.Features.Access.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Access.GetAccessRights;

public sealed class GetAccessRightsQueryHandler(IAccessRightsService accessRightsService)
    : IQueryHandler<GetAccessRightsQuery, Result<AccessRightsDto>>
{
    public async ValueTask<Result<AccessRightsDto>> Handle(GetAccessRightsQuery query, CancellationToken cancellationToken)
        => Result.Success(await accessRightsService.GetAsync(cancellationToken));
}
