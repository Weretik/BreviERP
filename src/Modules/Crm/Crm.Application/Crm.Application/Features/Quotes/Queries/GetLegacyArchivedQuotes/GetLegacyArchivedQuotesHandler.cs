using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Access;
using Crm.Application.Features.Quotes.Shared.Persistence;
using Crm.Application.Abstractions.Quotes;

namespace Crm.Application.Features.Quotes.Queries.GetLegacyArchivedQuotes;

public sealed class GetLegacyArchivedQuotesHandler(IQuoteStore store)
    : IQueryHandler<GetLegacyArchivedQuotesQuery, Result<LegacyArchivedQuotesResponse>>
{
    public async ValueTask<Result<LegacyArchivedQuotesResponse>> Handle(GetLegacyArchivedQuotesQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.Actor.CanDelete) return Result<LegacyArchivedQuotesResponse>.Forbidden(QuoteMessages.DeleteForbidden);
        var count = await store.Quotes.CountAsync(x => x.ArchivedAt != null, cancellationToken);
        return Result.Success(new LegacyArchivedQuotesResponse(count));
    }
}
