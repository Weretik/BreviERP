using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.GetQuoteVersions.Specifications;
using Crm.Application.Quotes.Persistence;
using Mediator;

namespace Crm.Application.Quotes.GetQuoteVersions;

public sealed class GetQuoteVersionsQueryHandler(IQuoteReadRepository repository)
    : IQueryHandler<GetQuoteVersionsQuery, Result<PagedResult<IReadOnlyList<QuoteVersionSummary>>>>
{
    public async ValueTask<Result<PagedResult<IReadOnlyList<QuoteVersionSummary>>>> Handle(
        GetQuoteVersionsQuery query, CancellationToken cancellationToken)
    {
        if (query.Page is < 1 or > 1_000_000 || query.PageSize is < 1 or > 100)
            return Result.Invalid(new ValidationError("page", "invalid_pagination"));
        if (query.Actor.UserId <= 0 || await repository.FirstOrDefaultAsync(
            new QuoteForVersionsSpec(query.QuoteId, query.Actor), cancellationToken) is null) return Result.NotFound();
        return Result.Success(await repository.VersionsAsync(query.QuoteId, query.Actor, query.Page, query.PageSize, cancellationToken));
    }
}
