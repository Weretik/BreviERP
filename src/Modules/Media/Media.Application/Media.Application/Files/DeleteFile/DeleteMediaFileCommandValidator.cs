namespace Media.Application.Files.DeleteFile;

public sealed class DeleteMediaFileCommandValidator : AbstractValidator<DeleteMediaFileCommand>
{
    public DeleteMediaFileCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
