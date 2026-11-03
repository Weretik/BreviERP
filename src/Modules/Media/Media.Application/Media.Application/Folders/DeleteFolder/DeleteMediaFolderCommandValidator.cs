namespace Media.Application.Folders.DeleteFolder;

public sealed class DeleteMediaFolderCommandValidator : AbstractValidator<DeleteMediaFolderCommand>
{
    public DeleteMediaFolderCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
