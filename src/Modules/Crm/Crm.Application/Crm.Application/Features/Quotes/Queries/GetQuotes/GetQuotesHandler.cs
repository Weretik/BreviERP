using BuildingBlocks.Domain.Identifiers.People;
using Crm.Domain.Quotes;
using Crm.Application.Features.Leads.Shared;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;

namespace Crm.Application.Features.Quotes.Queries.GetQuotes;

public sealed class GetQuotesHandler(IQuoteStore store) : IQueryHandler<GetQuotesQuery, Result<QuotePageResponse>>
{
    public async ValueTask<Result<QuotePageResponse>> Handle(GetQuotesQuery query, CancellationToken cancellationToken)
    {
        var quotes = store.Quotes.AsNoTracking().Where(x => x.ArchivedAt == null);
        if (query.CustomerPublicId is { } customer)
            quotes = quotes.Where(x => x.CustomerPublicId == customer);
        if (query.From is { } from)
        {
            var start = LeadClock.StartOf(from);
            quotes = quotes.Where(x => x.CreatedAt >= start);
        }
        if (query.To is { } to)
        {
            var end = LeadClock.StartOf(to.AddDays(1));
            quotes = quotes.Where(x => x.CreatedAt < end);
        }
        if (!query.Actor.CanSeeAll)
            quotes = quotes.Where(x => x.ResponsibleUserId == new UserId(query.Actor.UserId));
        else if (query.ResponsibleUserId is { } responsible)
            quotes = quotes.Where(x => x.ResponsibleUserId == new UserId(responsible));

        if (!string.IsNullOrEmpty(query.Status))
        {
            var status = Enum.Parse<QuoteStatus>(query.Status);
            quotes = quotes.Where(x => x.Status == status);
        }

        quotes = quotes.WhereSearchText(query.Search);

        var total = await quotes.CountAsync(cancellationToken);
        var rows = await quotes
            .OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new QuoteSummaryResponse(x.Id.Value, x.Number, x.Kind.ToString(), x.Status.ToString(),
                x.InternalName, x.CompanyName, new QuotePersonResponse(x.ResponsibleUserId.Value, x.ResponsibleName),
                x.Items.Count, x.UpdatedAt, x.LastSentAt, x.Version))
            .ToListAsync(cancellationToken);
        return Result.Success(new QuotePageResponse(rows, query.Page, query.PageSize, total));
    }
}
