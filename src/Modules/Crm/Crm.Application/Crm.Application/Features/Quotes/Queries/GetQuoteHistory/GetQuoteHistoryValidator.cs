namespace Crm.Application.Features.Quotes.Queries.GetQuoteHistory;

public sealed class GetQuoteHistoryValidator : AbstractValidator<GetQuoteHistoryQuery>
{
    public GetQuoteHistoryValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
    }
}
