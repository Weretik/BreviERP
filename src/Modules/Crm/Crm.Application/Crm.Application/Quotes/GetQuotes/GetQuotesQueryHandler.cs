using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Persistence;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.GetQuotes;

public sealed class GetQuotesQueryHandler(IQuoteStore store) : IQueryHandler<GetQuotesQuery, Result<QuotePageResponse>>
{
    public async ValueTask<Result<QuotePageResponse>> Handle(GetQuotesQuery query, CancellationToken cancellationToken)
    {
        var quotes = query.Archived
            ? store.Quotes.AsNoTracking().Where(x => x.ArchivedAt != null)
            : store.Quotes.AsNoTracking().Where(x => x.ArchivedAt == null);
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
                x.Items.Count, x.UpdatedAt, x.LastSentAt, x.ArchivedAt))
            .ToListAsync(cancellationToken);
        return Result.Success(new QuotePageResponse(rows, query.Page, query.PageSize, total));
    }
}
