using Media.Application.Features.Folders.Shared;
namespace Media.Application.Features.Folders.Commands.UpdateMediaFolder;

public sealed class UpdateMediaFolderValidator : AbstractValidator<UpdateMediaFolderCommand>
{
    public UpdateMediaFolderValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new FolderInputValidator());
    }
}
