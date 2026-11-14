using Crm.Domain.Quotes;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetQuotePeriod;

public sealed class GetQuotePeriodHandler(IQuoteStore store, TimeProvider clock)
    : IQueryHandler<GetQuotePeriodQuery, Result<QuotePeriodResponse>>
{
    public async ValueTask<Result<QuotePeriodResponse>> Handle(GetQuotePeriodQuery query,
        CancellationToken cancellationToken)
    {
        var today = LeadClock.KyivDate(clock.GetUtcNow());
        var to = query.To ?? today;
        var from = query.From ?? new DateOnly(to.Year, to.Month, 1);
        var start = LeadClock.StartOf(from);
        var end = LeadClock.StartOf(to.AddDays(1));

        var quotes = store.Quotes.AsNoTracking().Where(x => x.ArchivedAt == null);
        if (!query.Actor.CanSeeAll)
            quotes = quotes.Where(x => x.ResponsibleUserId == query.Actor.UserId);

        var sent = store.QuoteEvents.AsNoTracking()
            .Where(e => e.Kind == QuoteEventKind.Sent && e.At >= start && e.At < end)
            .Select(e => e.QuoteId);
        var rows = await quotes
            .Where(x => (x.CreatedAt >= start && x.CreatedAt < end) || sent.Contains(x.Id))
            .Select(x => new QuotePeriodRow(x.ResponsibleUserId, x.ResponsibleName,
                x.CreatedAt >= start && x.CreatedAt < end, sent.Contains(x.Id)))
            .ToListAsync(cancellationToken);

        return Result.Success(QuotePeriodCounter.Count(from, to, rows));
    }
}
