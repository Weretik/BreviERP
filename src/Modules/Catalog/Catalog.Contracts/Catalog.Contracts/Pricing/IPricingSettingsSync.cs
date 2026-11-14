namespace Catalog.Contracts.Pricing;

/// <summary>
/// «Параметри ціноутворення» changed in Reference: Catalog re-reads them into its sewing settings and PPE rule
/// versions. Reference calls it from its outbox (PLT-003 R-09), so a failure is tried again; it re-reads every
/// parameter, so a repeated call is harmless.
/// </summary>
public interface IPricingSettingsSync
{
    /// <returns>Null when Catalog has the numbers; otherwise why not (the outbox tries again).</returns>
    Task<string?> SettingsChangedAsync(CancellationToken cancellationToken);
}
