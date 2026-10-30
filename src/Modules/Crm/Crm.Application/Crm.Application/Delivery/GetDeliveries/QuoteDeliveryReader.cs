using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.Outbox;

namespace Crm.Application.Delivery.GetDeliveries;

public sealed class QuoteDeliveryReader(IQuoteDeliveryStore store)
{
    public Task<Result<PagedResult<IReadOnlyList<QuoteDeliveryDetail>>>> ListAsync(Guid quoteId, CrmActor actor,
        int page, int pageSize, CancellationToken ct) => store.ListAsync(quoteId, actor, page, pageSize, ct);
    public Task<Result<QuoteDeliveryDetail>> GetAsync(Guid quoteId, Guid deliveryId, CrmActor actor, CancellationToken ct)
        => store.GetAsync(quoteId, deliveryId, actor, ct);
}
