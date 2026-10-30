using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;
using Crm.Application.Quotes.Introductions;
using Crm.Infrastructure.Persistence.Quotes.Idempotency;

namespace Crm.Infrastructure.Persistence.Introductions;

public sealed class IntroductionQuoteReplayReader(CrmDbContext db) : IIntroductionQuoteReplayReader
{
    public Task<Result<QuoteDetail>?> ReadAsync(Guid requestId, CrmActor actor,
        IntroductionQuoteApplicationIntent intent, CancellationToken cancellationToken)
        => new QuoteMutationJournal(db).ReadReplayAsync("CreateQuote", requestId, actor,
            QuoteMutationJournal.Encode("CreateQuote", requestId, actor, intent), cancellationToken);
}
