using Identity.Application.Features.Access.Shared;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Access.Commands.SaveRoleAccess;

public sealed class SaveRoleAccessHandler(IAccessRightsService accessRightsService)
    : ICommandHandler<SaveRoleAccessCommand, Result<AccessRoleDto>>
{
    public async ValueTask<Result<AccessRoleDto>> Handle(SaveRoleAccessCommand command, CancellationToken cancellationToken)
    {
        var grants = command.Grants.ToDictionary(x => x.Key, x => AccessActions.Join(x.Value), StringComparer.Ordinal);
        return await accessRightsService.SaveAsync(command.Role, command.Version, grants, cancellationToken);
    }
}
