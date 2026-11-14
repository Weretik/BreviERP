using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;

namespace Crm.Application.Features.Quotes.Queries.GetQuotePeriod;

/// <summary>
/// «КП за місяць» of the director's «Робочий стіл» (DESK-001): КП created and sent in a Kyiv period, both days included;
/// by default the current Kyiv month up to today. A Manager counts only their own.
/// </summary>
public sealed record GetQuotePeriodQuery(QuoteActor Actor, DateOnly? From, DateOnly? To)
    : IQuery<Result<QuotePeriodResponse>>;

public sealed class GetQuotePeriodValidator : AbstractValidator<GetQuotePeriodQuery>
{
    public GetQuotePeriodValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x).Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("Початок періоду пізніше за кінець.");
        RuleFor(x => x).Must(x => x.From is null || x.To is null || x.To.Value.DayNumber - x.From.Value.DayNumber <= 3660)
            .WithMessage("Період не довший за 10 років.");
    }
}
