using Identity.Application.Abstractions;

namespace Identity.Application.Features.Access.Commands.DeleteRole;

public sealed class DeleteRoleHandler(IAccessRightsService accessRightsService)
    : ICommandHandler<DeleteRoleCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteRoleCommand command, CancellationToken cancellationToken)
        => await accessRightsService.DeleteRoleAsync(command.Role, cancellationToken);
}
