using Ardalis.Result;
using Crm.Application.Customers.Access;
using Crm.Application.Delivery.Outbox;

namespace Crm.Application.Delivery.ResolveOutcome;

public sealed class ResolveQuoteDeliveryHandler(IQuoteDeliveryStore store)
{
    public Task<Result<QuoteDeliveryDetail>> HandleAsync(ResolveQuoteDeliveryCommand command, CrmActor actor, CancellationToken ct)
    {
        if (actor.UserId <= 0) return Task.FromResult(Result<QuoteDeliveryDetail>.Forbidden());
        if (command is null || !new ResolveQuoteDeliveryValidator().Validate(command).IsValid)
            return Task.FromResult(Result<QuoteDeliveryDetail>.Invalid(new ValidationError("resolution", "delivery_resolution_invalid")));
        return store.ResolveAsync(command, actor, ct);
    }
}
