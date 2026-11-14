using Identity.Application.Abstractions;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnPassword;

public sealed class ChangeOwnPasswordHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnPasswordCommand command, CancellationToken cancellationToken)
        => await profileService.ChangePasswordAsync(command.CurrentPassword, command.NewPassword, cancellationToken);
}
