using Identity.Application.Abstractions;

namespace Identity.Application.Features.Users.Commands.SetStaffPassword;

public sealed class SetStaffPasswordHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<SetStaffPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(SetStaffPasswordCommand command, CancellationToken cancellationToken)
        => await staffAccountService.SetPasswordAsync(command.UserId, command.Password, cancellationToken);
}
