using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.EnqueueQuote;
using Crm.Application.Delivery.Provider;
using Crm.Application.Delivery.Readiness;

namespace Crm.Application.Delivery.Outbox;

public interface IQuoteDeliveryStore
{
    Task<Result<QuoteDeliveryDetail>> EnqueueAsync(EnqueueQuoteDeliveryCommand command, CrmActor actor,
        IQuoteDeliveryIdentity identity, IQuoteDeliveryReadiness readiness, CancellationToken ct);
    Task<Result<PagedResult<IReadOnlyList<QuoteDeliveryDetail>>>> ListAsync(Guid quoteId, CrmActor actor, int page, int pageSize, CancellationToken ct);
    Task<Result<QuoteDeliveryDetail>> GetAsync(Guid quoteId, Guid deliveryId, CrmActor actor, CancellationToken ct);
    Task<QuoteDeliveryClaim?> ClaimAsync(IQuoteDeliveryIdentity identity, Func<string, bool> canProcessRecipient, CancellationToken ct);
    Task<bool> CompleteAsync(Guid deliveryId, Guid leaseToken, QuoteDeliveryProviderResult outcome, CancellationToken ct);
}
