using Identity.Application.Abstractions;
using Identity.Application.Features.Avatars.Shared;
using Identity.Application.Abstractions.Profile;

namespace Identity.Application.Features.Profile.Commands.UploadOwnAvatar;

public sealed class UploadOwnAvatarHandler(IProfileService profileService)
    : ICommandHandler<UploadOwnAvatarCommand, Result<AvatarResult>>
{
    public async ValueTask<Result<AvatarResult>> Handle(UploadOwnAvatarCommand command, CancellationToken cancellationToken)
        => await profileService.UploadAvatarAsync(command.Upload, cancellationToken);
}
