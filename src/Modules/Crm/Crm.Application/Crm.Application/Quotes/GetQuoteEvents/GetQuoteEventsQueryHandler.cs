using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.GetQuoteEvents.Specifications;
using Crm.Application.Quotes.Persistence;
using Mediator;

namespace Crm.Application.Quotes.GetQuoteEvents;

public sealed class GetQuoteEventsQueryHandler(IQuoteReadRepository repository)
    : IQueryHandler<GetQuoteEventsQuery, Result<PagedResult<IReadOnlyList<QuoteEventView>>>>
{
    public async ValueTask<Result<PagedResult<IReadOnlyList<QuoteEventView>>>> Handle(
        GetQuoteEventsQuery query, CancellationToken cancellationToken)
    {
        if (query.Page is < 1 or > 1_000_000 || query.PageSize is < 1 or > 100)
            return Result.Invalid(new ValidationError("page", "invalid_pagination"));
        if (query.Actor.UserId <= 0 || await repository.FirstOrDefaultAsync(
            new QuoteForEventsSpec(query.QuoteId, query.Actor), cancellationToken) is null) return Result.NotFound();
        return Result.Success(await repository.EventsAsync(query.QuoteId, query.Actor, query.Page, query.PageSize, cancellationToken));
    }
}
