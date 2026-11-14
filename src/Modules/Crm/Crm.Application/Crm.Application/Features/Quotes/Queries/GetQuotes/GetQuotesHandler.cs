using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetQuotes;

public sealed class GetQuotesHandler(IQuoteStore store) : IQueryHandler<GetQuotesQuery, Result<QuotePageResponse>>
{
    public async ValueTask<Result<QuotePageResponse>> Handle(GetQuotesQuery query, CancellationToken cancellationToken)
    {
        var quotes = store.Quotes.AsNoTracking().Where(x => x.ArchivedAt == null);
        if (query.CustomerId is { } customerId)
            quotes = quotes.Where(x => x.CustomerId == customerId);
        if (!query.Actor.CanSeeAll)
            quotes = quotes.Where(x => x.ResponsibleUserId == query.Actor.UserId);
        else if (query.ResponsibleUserId is { } responsible)
            quotes = quotes.Where(x => x.ResponsibleUserId == responsible);

        if (!string.IsNullOrEmpty(query.Status))
        {
            var status = Enum.Parse<QuoteStatus>(query.Status);
            quotes = quotes.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            quotes = quotes.Where(x => x.Number.ToLower().Contains(search) ||
                x.InternalName.ToLower().Contains(search) ||
                (x.CompanyName != null && x.CompanyName.ToLower().Contains(search)) ||
                (x.RecipientName != null && x.RecipientName.ToLower().Contains(search)));
        }

        var total = await quotes.CountAsync(cancellationToken);
        var rows = await quotes
            .OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new QuoteSummaryResponse(x.Id, x.Number, x.Kind.ToString(), x.Status.ToString(),
                x.InternalName, x.CompanyName, new QuotePersonResponse(x.ResponsibleUserId, x.ResponsibleName),
                x.Items.Count, x.UpdatedAt, x.LastSentAt, x.Version))
            .ToListAsync(cancellationToken);
        return Result.Success(new QuotePageResponse(rows, query.Page, query.PageSize, total));
    }
}
