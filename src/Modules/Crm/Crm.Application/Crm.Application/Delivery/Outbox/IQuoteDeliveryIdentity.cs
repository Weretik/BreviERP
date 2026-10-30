namespace Crm.Application.Delivery.Outbox;

public interface IQuoteDeliveryIdentity
{
    string ProviderKey { get; }
    string SenderAddress { get; }
    int LeaseSeconds { get; }
    // Optional dispatch scope is applied before candidate limits; readiness remains authoritative.
    string? RecipientRestriction => null;
}
