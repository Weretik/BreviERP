using Crm.Application.Delivery.Outbox;

namespace Crm.Infrastructure.Delivery.Readiness;

public sealed class ConfiguredQuoteDeliveryIdentity(QuoteDeliveryProviderOptions options) : IQuoteDeliveryIdentity
{
    public string ProviderKey => options.ProviderKey;
    public string SenderAddress => options.SenderAddress;
    public int LeaseSeconds => options.LeaseSeconds;
    public string? RecipientRestriction => options.SandboxOnly ? options.TestRecipientAddress : null;
}
