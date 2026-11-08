using Identity.Application.Features.Access.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Access.CreateRole;

public sealed class CreateRoleCommandHandler(IAccessRightsService accessRightsService)
    : ICommandHandler<CreateRoleCommand, Result<AccessRoleDto>>
{
    public async ValueTask<Result<AccessRoleDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
        => await accessRightsService.CreateRoleAsync(command.Title.Trim(), cancellationToken);
}
