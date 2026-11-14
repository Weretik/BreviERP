using BuildingBlocks.Application.Outbox;
using Catalog.Contracts.Pricing;

namespace Reference.Infrastructure.Adapters.Catalog;

public sealed record PricingSettingsChangedPayload;

/// <summary>Hands the saved «Параметри ціноутворення» to Catalog; a refusal is tried again (PLT-003 R-09).</summary>
internal sealed class PricingSettingsChangedOutboxHandler(IPricingSettingsSync sync) : IOutboxHandler
{
    public const string MessageType = "reference.pricing-settings.changed";

    public string Type => MessageType;

    public Task<string?> HandleAsync(string payload, CancellationToken cancellationToken) =>
        sync.SettingsChangedAsync(cancellationToken);
}
