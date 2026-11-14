using Identity.Application.Abstractions;
using Identity.Application.Abstractions.Profile;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnEmail;

public sealed class ChangeOwnEmailHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnEmailCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnEmailCommand command, CancellationToken cancellationToken)
        => await profileService.ChangeEmailAsync(command.Email, command.CurrentPassword, cancellationToken);
}
