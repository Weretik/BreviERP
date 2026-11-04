namespace Crm.Application.Quotes.CopyQuote;

public sealed class CopyQuoteCommandValidator : AbstractValidator<CopyQuoteCommand>
{
    public CopyQuoteCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
