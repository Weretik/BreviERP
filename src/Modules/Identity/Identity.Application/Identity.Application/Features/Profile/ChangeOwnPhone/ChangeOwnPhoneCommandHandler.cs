using Identity.Application.Services;

namespace Identity.Application.Features.Profile.ChangeOwnPhone;

public sealed class ChangeOwnPhoneCommandHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnPhoneCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnPhoneCommand command, CancellationToken cancellationToken)
        => await profileService.ChangePhoneAsync(command.PhoneNumber, cancellationToken);
}
