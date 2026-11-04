using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Persistence;

namespace Crm.Application.Quotes.History;

public sealed class GetQuoteHistoryQueryHandler(IQuoteStore store)
    : IQueryHandler<GetQuoteHistoryQuery, Result<IReadOnlyList<QuoteEventResponse>>>
{
    public const int MaxEvents = 500;

    public async ValueTask<Result<IReadOnlyList<QuoteEventResponse>>> Handle(GetQuoteHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var responsible = await store.Quotes.AsNoTracking().Where(x => x.Id == query.Id)
            .Select(x => (int?)x.ResponsibleUserId).SingleOrDefaultAsync(cancellationToken);
        if (responsible is null || !(query.Actor.CanSeeAll || responsible == query.Actor.UserId))
            return Result.NotFound(QuoteMessages.NotFound);

        var events = await store.QuoteEvents.AsNoTracking().Where(x => x.QuoteId == query.Id)
            .OrderByDescending(x => x.At).ThenByDescending(x => x.Id).Take(MaxEvents)
            .Select(x => new QuoteEventResponse(x.Id, x.Kind.ToString(), x.At,
                new QuotePersonResponse(x.ActorUserId, x.ActorName), x.Details))
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<QuoteEventResponse>>(events);
    }
}
