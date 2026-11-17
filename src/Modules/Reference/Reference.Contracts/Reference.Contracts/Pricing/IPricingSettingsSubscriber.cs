namespace Reference.Contracts.Pricing;

/// <summary>
/// «Параметри ціноутворення» or a PPE markup rule changed in Reference. Reference tells every subscriber from its outbox
/// (PLT-003 R-09, ADR-0019), so a failure is tried again; a subscriber re-reads every parameter, so a repeated call is
/// harmless.
/// </summary>
public interface IPricingSettingsSubscriber
{
    /// <returns>Null when the subscriber has the numbers; otherwise why not (the outbox tries again).</returns>
    Task<string?> SettingsChangedAsync(CancellationToken cancellationToken);
}
