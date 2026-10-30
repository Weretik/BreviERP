using Crm.Application.Delivery.Outbox;
using Crm.Application.Delivery.Provider;
using Crm.Application.Delivery.Readiness;

namespace Crm.Application.Delivery.Processing;

public sealed class QuoteDeliveryProcessor(IQuoteDeliveryStore store, IQuoteDeliveryProvider provider,
    IQuoteDeliveryIdentity identity, IQuoteDeliveryReadiness readiness)
{
    public async Task<bool> ProcessOneAsync(CancellationToken ct)
    {
        if (provider.Capabilities.ProviderKey != identity.ProviderKey) return false;
        var claim = await store.ClaimAsync(identity, recipient => readiness.Assess(recipient).CanSubmit, ct);
        if (claim is null) return false;
        QuoteDeliveryProviderResult result;
        try
        {
            result = claim.Reconcile
                ? await provider.ReconcileAsync(claim.DeliveryId, ct)
                : await provider.SubmitAsync(claim.Submission, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Leave the committed lease uncertain. A later worker reconciles instead of resubmitting.
            throw;
        }
        catch (Exception)
        {
            result = new QuoteDeliveryProviderResult(QuoteDeliveryProviderOutcome.Unknown, null, "delivery_outcome_unknown");
        }
        await store.CompleteAsync(claim.DeliveryId, claim.LeaseToken, result, ct);
        return true;
    }
}
