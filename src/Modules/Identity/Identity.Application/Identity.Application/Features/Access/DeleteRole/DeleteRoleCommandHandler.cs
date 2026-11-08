using Identity.Application.Services;

namespace Identity.Application.Features.Access.DeleteRole;

public sealed class DeleteRoleCommandHandler(IAccessRightsService accessRightsService)
    : ICommandHandler<DeleteRoleCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
        => await accessRightsService.DeleteRoleAsync(command.Role, cancellationToken);
}
