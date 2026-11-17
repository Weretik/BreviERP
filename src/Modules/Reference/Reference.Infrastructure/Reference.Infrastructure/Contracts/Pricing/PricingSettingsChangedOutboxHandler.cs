using BuildingBlocks.Application.Outbox;
using Reference.Contracts.Pricing;

namespace Reference.Infrastructure.Contracts.Pricing;

public sealed record PricingSettingsChangedPayload;

/// <summary>
/// Tells every <see cref="IPricingSettingsSubscriber"/> that «Параметри ціноутворення» were saved; a refusal is tried
/// again (PLT-003 R-09, ADR-0019).
/// </summary>
internal sealed class PricingSettingsChangedOutboxHandler(IEnumerable<IPricingSettingsSubscriber> subscribers)
    : IOutboxHandler
{
    public const string MessageType = "reference.pricing-settings.changed";

    public string Type => MessageType;

    public async Task<string?> HandleAsync(string payload, CancellationToken cancellationToken)
    {
        foreach (var subscriber in subscribers)
            if (await subscriber.SettingsChangedAsync(cancellationToken) is { } refusal)
                return refusal;
        return null;
    }
}
