namespace Catalog.Application.Features.Media.Delete.Validators;

public sealed class DeleteMediaFileCommandValidator : AbstractValidator<DeleteMediaFileCommand>
{
    public DeleteMediaFileCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
