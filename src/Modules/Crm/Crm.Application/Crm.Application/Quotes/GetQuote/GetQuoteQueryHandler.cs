using Crm.Application.Quotes.Access;
using Crm.Application.Quotes.Contracts;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;

namespace Crm.Application.Quotes.GetQuote;

public sealed class GetQuoteQueryHandler(IQuoteStore store) : IQueryHandler<GetQuoteQuery, Result<QuoteResponse>>
{
    public async ValueTask<Result<QuoteResponse>> Handle(GetQuoteQuery query, CancellationToken cancellationToken)
    {
        var quote = await store.Quotes.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Options)
            .Include(x => x.Items).ThenInclude(x => x.Lines)
            .AsSplitQuery()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        return quote is null || !query.Actor.CanAccess(quote)
            ? Result.NotFound(QuoteMessages.NotFound)
            : Result.Success(quote.ToResponse());
    }
}
