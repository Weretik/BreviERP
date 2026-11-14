namespace Crm.Application.Features.Quotes.Queries.GetQuote;

public sealed class GetQuoteValidator : AbstractValidator<GetQuoteQuery>
{
    public GetQuoteValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
