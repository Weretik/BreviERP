namespace Media.Application.Folders.UpdateFolder;

public sealed class UpdateMediaFolderCommandValidator : AbstractValidator<UpdateMediaFolderCommand>
{
    public UpdateMediaFolderCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new FolderInputValidator());
    }
}
