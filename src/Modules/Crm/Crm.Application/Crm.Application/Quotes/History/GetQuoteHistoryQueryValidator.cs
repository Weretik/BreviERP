namespace Crm.Application.Quotes.History;

public sealed class GetQuoteHistoryQueryValidator : AbstractValidator<GetQuoteHistoryQuery>
{
    public GetQuoteHistoryQueryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
