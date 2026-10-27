using Ardalis.Result;
using Crm.Application.Customers.Access;

namespace Crm.Application.Quotes.Pricing;

public interface IQuotePriceBasisStore
{
    Task<Result<QuotePricePreview>> ExecuteAsync(
        Guid requestId, Guid? quoteId, CrmActor actor, QuotePriceIntent intent,
        Func<CancellationToken, Task<Result<QuotePricePreview>>> work,
        CancellationToken cancellationToken);

    Task<QuotePricePreview?> FindAsync(
        Guid token, Guid? quoteId, CrmActor actor, CancellationToken cancellationToken);
}
