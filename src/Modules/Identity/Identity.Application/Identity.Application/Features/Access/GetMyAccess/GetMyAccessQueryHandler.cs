using Identity.Application.Features.Access.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Access.GetMyAccess;

public sealed class GetMyAccessQueryHandler(IAccessRightsService accessRightsService)
    : IQueryHandler<GetMyAccessQuery, Result<MyAccessDto>>
{
    public async ValueTask<Result<MyAccessDto>> Handle(GetMyAccessQuery query, CancellationToken cancellationToken)
        => Result.Success(await accessRightsService.GetMineAsync(cancellationToken));
}
