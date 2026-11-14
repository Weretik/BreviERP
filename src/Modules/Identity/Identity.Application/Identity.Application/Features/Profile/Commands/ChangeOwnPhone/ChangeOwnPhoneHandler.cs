using Identity.Application.Abstractions;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnPhone;

public sealed class ChangeOwnPhoneHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnPhoneCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnPhoneCommand command, CancellationToken cancellationToken)
        => await profileService.ChangePhoneAsync(command.PhoneNumber, cancellationToken);
}
