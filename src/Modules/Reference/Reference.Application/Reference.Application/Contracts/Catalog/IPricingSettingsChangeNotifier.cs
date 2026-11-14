namespace Reference.Application.Contracts.Catalog;

/// <summary>
/// Called after a save that changed «Параметри ціноутворення» or a PPE markup rule. That save already holds the
/// outbox message for Catalog (PLT-003 R-09); this tries it at once. A failure stays in the outbox and never undoes the
/// Reference change.
/// </summary>
public interface IPricingSettingsChangeNotifier
{
    Task PricingSettingsChangedAsync(CancellationToken cancellationToken);
}
