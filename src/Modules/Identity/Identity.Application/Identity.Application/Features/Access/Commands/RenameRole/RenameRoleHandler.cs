using Identity.Application.Features.Access.Shared;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Access.Commands.RenameRole;

public sealed class RenameRoleHandler(IAccessRightsService accessRightsService)
    : ICommandHandler<RenameRoleCommand, Result<AccessRoleDto>>
{
    public async ValueTask<Result<AccessRoleDto>> Handle(RenameRoleCommand command, CancellationToken cancellationToken)
        => await accessRightsService.RenameRoleAsync(command.Role, command.Title.Trim(), cancellationToken);
}
