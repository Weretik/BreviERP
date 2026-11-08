using Identity.Application.Features.Access.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Access.SaveRoleAccess;

public sealed class SaveRoleAccessCommandHandler(IAccessRightsService accessRightsService)
    : ICommandHandler<SaveRoleAccessCommand, Result<AccessRoleDto>>
{
    public async ValueTask<Result<AccessRoleDto>> Handle(SaveRoleAccessCommand command, CancellationToken cancellationToken)
    {
        var grants = command.Grants.ToDictionary(x => x.Key, x => AccessActions.Join(x.Value), StringComparer.Ordinal);
        return await accessRightsService.SaveAsync(command.Role, command.Version, grants, cancellationToken);
    }
}
