using FluentValidation;

namespace Identity.Application.Features.Profile.Commands.UploadOwnAvatar;

/// <summary>Only presence here; size, format and resolution are checked when the image is decoded.</summary>
public sealed class UploadOwnAvatarValidator : AbstractValidator<UploadOwnAvatarCommand>
{
    public UploadOwnAvatarValidator()
    {
        RuleFor(x => x.Upload).NotNull();
        RuleFor(x => x.Upload.Length).GreaterThan(0).WithName("file").When(x => x.Upload is not null);
    }
}
