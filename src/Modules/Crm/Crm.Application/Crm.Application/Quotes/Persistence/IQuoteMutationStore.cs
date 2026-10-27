using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Quotes.Contracts.Responses;

namespace Crm.Application.Quotes.Persistence;

public interface IQuoteMutationStore
{
    Task<Result<QuoteDetail>> ExecuteAsync(
        string operation, Guid requestId, CrmActor actor, object intent,
        Func<IQuoteWriteSession, CancellationToken, Task<Result<QuoteDetail>>> work,
        CancellationToken cancellationToken);
}
