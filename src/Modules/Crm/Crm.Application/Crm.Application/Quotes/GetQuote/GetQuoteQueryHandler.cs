using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.GetQuote.Specifications;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Mediator;

namespace Crm.Application.Quotes.GetQuote;

public sealed class GetQuoteQueryHandler(IQuoteReadRepository repository)
    : IQueryHandler<GetQuoteQuery, Result<QuoteDetail>>
{
    public async ValueTask<Result<QuoteDetail>> Handle(GetQuoteQuery query, CancellationToken cancellationToken)
    {
        if (query.QuoteId == Guid.Empty || query.Actor.UserId <= 0) return Result.NotFound();
        var quote = await repository.FirstOrDefaultAsync(new QuoteDetailSpec(query.QuoteId, query.Actor), cancellationToken);
        if (quote?.CurrentVersionId is not { } versionId) return Result.NotFound();
        var version = await repository.FindVersionAsync(versionId, cancellationToken);
        return version is null ? Result.NotFound() : Result.Success(QuoteDetailMapper.Map(quote, version,
            query.Actor.IsAdmin || quote.OwnerId.Value == query.Actor.UserId));
    }
}
