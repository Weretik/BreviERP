using Catalog.Contracts.Pricing;
using Reference.Application.Contracts.Catalog;

namespace Reference.Infrastructure.Adapters.Catalog;

/// <summary>Tells Catalog that «Параметри ціноутворення» changed; Catalog logs and never throws.</summary>
internal sealed class PricingSettingsChangeNotifier(IPricingSettingsSync sync) : IPricingSettingsChangeNotifier
{
    public Task PricingSettingsChangedAsync(CancellationToken cancellationToken) =>
        sync.SettingsChangedAsync(cancellationToken);
}
