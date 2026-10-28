using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.GetQuotes.Specifications;
using Crm.Application.Quotes.Persistence;
using Mediator;

namespace Crm.Application.Quotes.GetQuotes;

public sealed class GetQuotesQueryHandler(IQuoteReadRepository repository)
    : IQueryHandler<GetQuotesQuery, PagedResult<IReadOnlyList<QuoteSummary>>>
{
    public async ValueTask<PagedResult<IReadOnlyList<QuoteSummary>>> Handle(
        GetQuotesQuery query, CancellationToken cancellationToken)
    {
        if (query.Actor.UserId <= 0 || query.Page is < 1 or > 1_000_000 ||
            query.PageSize is < 1 or > 100 || query.Search?.Length > 300 ||
            query.State is not (null or "draft" or "ready" or "sent") ||
            query.OwnerId is <= 0 || query.SourceQuoteId == Guid.Empty ||
            (query.CreatedFrom is not null && query.CreatedBefore is not null && query.CreatedFrom >= query.CreatedBefore))
            throw new ArgumentOutOfRangeException(nameof(query), "invalid_pagination_or_filter");
        var count = await repository.CountAsync(new QuoteCountSpec(query.Actor, query.Search,
            query.State, query.Archived, query.OwnerId, query.CreatedFrom, query.CreatedBefore, query.SourceQuoteId), cancellationToken);
        var rows = await repository.ListAsync(new QuotePageSpec(query.Actor, query.Search,
            query.State, query.Archived, query.OwnerId, query.Page, query.PageSize,
            query.CreatedFrom, query.CreatedBefore, query.SourceQuoteId), cancellationToken);
        return new PagedResult<IReadOnlyList<QuoteSummary>>(new PagedInfo(
            query.Page, query.PageSize, (long)Math.Ceiling(count / (double)query.PageSize), count), rows);
    }
}
