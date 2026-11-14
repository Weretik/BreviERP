using Identity.Application.Abstractions;

namespace Identity.Application.Features.Users.Commands.SetStaffBlocked;

public sealed class SetStaffBlockedHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<SetStaffBlockedCommand, Result>
{
    public async ValueTask<Result> Handle(SetStaffBlockedCommand command, CancellationToken cancellationToken)
        => await staffAccountService.SetBlockedAsync(command.UserId, command.Blocked, cancellationToken);
}
