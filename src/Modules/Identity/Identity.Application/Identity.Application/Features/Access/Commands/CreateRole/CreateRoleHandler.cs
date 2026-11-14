using Identity.Application.Features.Access.Shared;
using Identity.Application.Abstractions;

namespace Identity.Application.Features.Access.Commands.CreateRole;

public sealed class CreateRoleHandler(IAccessRightsService accessRightsService)
    : ICommandHandler<CreateRoleCommand, Result<AccessRoleDto>>
{
    public async ValueTask<Result<AccessRoleDto>> Handle(CreateRoleCommand command, CancellationToken cancellationToken)
        => await accessRightsService.CreateRoleAsync(command.Title.Trim(), cancellationToken);
}
