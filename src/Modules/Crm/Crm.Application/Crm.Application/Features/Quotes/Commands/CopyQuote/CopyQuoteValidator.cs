namespace Crm.Application.Features.Quotes.Commands.CopyQuote;

public sealed class CopyQuoteValidator : AbstractValidator<CopyQuoteCommand>
{
    public CopyQuoteValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
