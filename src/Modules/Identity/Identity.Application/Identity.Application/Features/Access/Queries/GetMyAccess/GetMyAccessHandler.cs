using Identity.Application.Features.Access.Shared;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Access.Queries.GetMyAccess;

public sealed class GetMyAccessHandler(IAccessRightsService accessRightsService)
    : IQueryHandler<GetMyAccessQuery, Result<MyAccessDto>>
{
    public async ValueTask<Result<MyAccessDto>> Handle(GetMyAccessQuery query, CancellationToken cancellationToken)
        => Result.Success(await accessRightsService.GetMineAsync(cancellationToken));
}
