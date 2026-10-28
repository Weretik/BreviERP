using Ardalis.Result;
using Crm.Application.Quotes.Ownership;
using Mediator;

namespace Crm.Application.Quotes.GetQuoteAssignees;

public sealed class GetQuoteAssigneesQueryHandler(IQuoteAssigneeReader assignees)
    : IQueryHandler<GetQuoteAssigneesQuery, Result<PagedResult<IReadOnlyList<QuoteAssignee>>>>
{
    public async ValueTask<Result<PagedResult<IReadOnlyList<QuoteAssignee>>>> Handle(
        GetQuoteAssigneesQuery query, CancellationToken cancellationToken)
    {
        if (query.Actor.UserId <= 0) return Result.Forbidden();
        if (query.Page is < 1 or > 1_000_000 || query.PageSize is < 1 or > 100 || query.Search?.Length > 200)
            return Result.Invalid(new ValidationError("page", "invalid_pagination_or_search"));
        return Result.Success(await assignees.SearchAsync(query.Search, query.Page, query.PageSize, cancellationToken));
    }
}
