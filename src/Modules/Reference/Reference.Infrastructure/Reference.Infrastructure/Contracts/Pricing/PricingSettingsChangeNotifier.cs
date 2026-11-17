using BuildingBlocks.Application.Outbox;
using Reference.Application.Abstractions;

namespace Reference.Infrastructure.Contracts.Pricing;

/// <summary>Tries at once the «Параметри ціноутворення» messages the last saves wrote (PLT-003 R-09).</summary>
internal sealed class PricingSettingsChangeNotifier(PricingSettingsOutboxInterceptor saved, IOutboxDispatcher outbox)
    : IPricingSettingsChangeNotifier
{
    public Task PricingSettingsChangedAsync(CancellationToken cancellationToken) =>
        outbox.DispatchAsync(saved.TakeSaved(), CancellationToken.None);
}
