namespace Catalog.Contracts.Pricing;

/// <summary>
/// «Параметри ціноутворення» changed in Reference: Catalog re-reads them into its sewing settings and PPE rule
/// versions. A failure is logged and never thrown, so the saved Reference change stays.
/// </summary>
public interface IPricingSettingsSync
{
    Task SettingsChangedAsync(CancellationToken cancellationToken);
}
