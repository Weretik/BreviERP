using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Persistence;
using Crm.Application.Quotes.ProductionLinks;
using Crm.Application.Quotes.Staff;

namespace Crm.Application.Quotes.LegacyArchive;

public sealed class GetLegacyArchivedQuotesQueryHandler(IQuoteStore store)
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
