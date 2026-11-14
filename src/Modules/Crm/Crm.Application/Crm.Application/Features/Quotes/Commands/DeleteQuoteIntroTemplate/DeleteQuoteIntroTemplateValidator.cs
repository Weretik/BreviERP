namespace Crm.Application.Features.Quotes.Commands.DeleteQuoteIntroTemplate;

public sealed class DeleteQuoteIntroTemplateValidator : AbstractValidator<DeleteQuoteIntroTemplateCommand>
{
    public DeleteQuoteIntroTemplateValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
