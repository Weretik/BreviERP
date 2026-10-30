namespace Crm.Application.Delivery.Readiness;

/// <summary>Fail-closed prerequisite check for one intended recipient; it does not send mail.</summary>
public interface IQuoteDeliveryReadiness
{
    QuoteDeliveryReadiness Assess(string recipientAddress);
}
