using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Readiness;

namespace Crm.Application.Delivery.EnqueueQuote;

public sealed class EnqueueQuoteDeliveryHandler(IQuoteDeliveryStore store, IQuoteDeliveryIdentity identity,
    IQuoteDeliveryReadiness readiness)
{
    public Task<Result<QuoteDeliveryDetail>> HandleAsync(EnqueueQuoteDeliveryCommand command, CrmActor actor, CancellationToken ct)
        => store.EnqueueAsync(command, actor, identity, readiness, ct);
}
