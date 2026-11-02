namespace Reference.Application.Contracts.Catalog;

/// <summary>
/// Catalog-owned boundary called after «Параметри ціноутворення» change, so Catalog prices use the new numbers.
/// The Reference change is already saved; a failed sync is logged and does not undo it.
/// </summary>
public interface IPricingSettingsChangeNotifier
{
    Task PricingSettingsChangedAsync(CancellationToken cancellationToken);
}
