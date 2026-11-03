namespace Media.Application.Folders.CreateFolder;

public sealed class CreateMediaFolderCommandValidator : AbstractValidator<CreateMediaFolderCommand>
{
    public CreateMediaFolderCommandValidator()
    {
        RuleFor(x => x.Input).NotNull().SetValidator(new FolderInputValidator());
    }
}
