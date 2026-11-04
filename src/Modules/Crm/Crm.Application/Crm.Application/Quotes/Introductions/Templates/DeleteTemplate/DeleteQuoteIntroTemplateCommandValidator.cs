namespace Crm.Application.Quotes.Introductions.Templates.DeleteTemplate;

public sealed class DeleteQuoteIntroTemplateCommandValidator : AbstractValidator<DeleteQuoteIntroTemplateCommand>
{
    public DeleteQuoteIntroTemplateCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
