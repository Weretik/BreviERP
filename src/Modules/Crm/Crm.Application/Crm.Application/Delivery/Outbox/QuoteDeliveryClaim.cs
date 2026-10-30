using Crm.Application.Delivery.Provider;

namespace Crm.Application.Delivery.Outbox;

public sealed record QuoteDeliveryClaim(Guid DeliveryId, Guid LeaseToken, bool Reconcile,
    QuoteDeliverySubmission Submission);
