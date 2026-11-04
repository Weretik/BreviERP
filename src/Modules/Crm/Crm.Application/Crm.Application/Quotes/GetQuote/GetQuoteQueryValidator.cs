namespace Crm.Application.Quotes.GetQuote;

public sealed class GetQuoteQueryValidator : AbstractValidator<GetQuoteQuery>
{
    public GetQuoteQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
