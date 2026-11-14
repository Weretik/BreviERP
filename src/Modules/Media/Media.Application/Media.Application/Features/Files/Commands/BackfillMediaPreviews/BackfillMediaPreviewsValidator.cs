namespace Media.Application.Features.Files.Commands.BackfillMediaPreviews;

public sealed class BackfillMediaPreviewsValidator : AbstractValidator<BackfillMediaPreviewsCommand>
{
    public BackfillMediaPreviewsValidator() => RuleFor(x => x.Limit).InclusiveBetween(1, 5000);
}
