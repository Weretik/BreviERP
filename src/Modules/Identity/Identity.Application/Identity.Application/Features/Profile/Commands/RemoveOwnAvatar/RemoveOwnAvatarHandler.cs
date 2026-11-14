using Identity.Application.Abstractions;
using Identity.Application.Abstractions.Profile;

namespace Identity.Application.Features.Profile.Commands.RemoveOwnAvatar;

public sealed class RemoveOwnAvatarHandler(IProfileService profileService)
    : ICommandHandler<RemoveOwnAvatarCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveOwnAvatarCommand command, CancellationToken cancellationToken)
        => await profileService.RemoveAvatarAsync(cancellationToken);
}
