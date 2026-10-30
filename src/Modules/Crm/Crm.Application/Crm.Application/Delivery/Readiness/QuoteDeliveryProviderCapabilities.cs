namespace Crm.Application.Delivery.Readiness;

/// <summary>Reported by the trusted registered adapter, never accepted from HTTP. A stable send key
/// means correlation identity, not native provider deduplication or permission to resend an unknown outcome.</summary>
public sealed record QuoteDeliveryProviderCapabilities(
    string ProviderKey, bool SupportsStableSendKey, bool SupportsLookupBySendKey);
