using Crm.Application.Delivery.Readiness;

namespace Crm.Application.Delivery.Provider;

public interface IQuoteDeliveryProvider
{
    QuoteDeliveryProviderCapabilities Capabilities { get; }
    /// <summary>The outbox owns first-attempt permission and replay. Never auto-retry unknown submissions.</summary>
    Task<QuoteDeliveryProviderResult> SubmitAsync(QuoteDeliverySubmission submission, CancellationToken cancellationToken);
    Task<QuoteDeliveryProviderResult> ReconcileAsync(Guid deliveryId, CancellationToken cancellationToken);
}
