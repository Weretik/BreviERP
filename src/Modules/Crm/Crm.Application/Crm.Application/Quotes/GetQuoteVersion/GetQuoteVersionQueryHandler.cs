using Ardalis.Result;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Mapping;
using Crm.Application.Quotes.Persistence;
using Mediator;

namespace Crm.Application.Quotes.GetQuoteVersion;

public sealed class GetQuoteVersionQueryHandler(IQuoteReadRepository repository)
    : IQueryHandler<GetQuoteVersionQuery, Result<QuoteVersionView>>
{
    public async ValueTask<Result<QuoteVersionView>> Handle(GetQuoteVersionQuery query, CancellationToken cancellationToken)
    {
        if (query.Actor.UserId <= 0) return Result.NotFound();
        var version = await repository.FindVersionAsync(query.QuoteId, query.VersionId, query.Actor, cancellationToken);
        return version is null ? Result.NotFound() : Result.Success(QuoteDetailMapper.MapVersion(version));
    }
}
