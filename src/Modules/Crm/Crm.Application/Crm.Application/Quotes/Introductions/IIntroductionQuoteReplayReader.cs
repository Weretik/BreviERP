using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;

namespace Crm.Application.Quotes.Introductions;

public interface IIntroductionQuoteReplayReader
{
    Task<Result<QuoteDetail>?> ReadAsync(Guid requestId, CrmActor actor,
        IntroductionQuoteApplicationIntent intent, CancellationToken cancellationToken);
}
