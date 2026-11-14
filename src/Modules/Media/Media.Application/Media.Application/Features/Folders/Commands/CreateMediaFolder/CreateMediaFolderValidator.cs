using Media.Application.Features.Folders.Shared;
namespace Media.Application.Features.Folders.Commands.CreateMediaFolder;

public sealed class CreateMediaFolderValidator : AbstractValidator<CreateMediaFolderCommand>
{
    public CreateMediaFolderValidator()
    {
        RuleFor(x => x.Input).NotNull().SetValidator(new FolderInputValidator());
    }
}
