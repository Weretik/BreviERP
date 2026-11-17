namespace Reference.Contracts.Pricing;

/// <summary>
/// Implemented by every module whose records hold Reference pricing inputs (a Catalog product composition), so
/// Reference does not delete a fabric, accessory or operation in use. Reference asks every registered source and
/// knows none of the modules (ADR-0019).
/// </summary>
public interface IPricingInputUsageSource
{
    Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken);

    Task<bool> IsAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken);

    Task<bool> IsOperationUsedAsync(int operationId, CancellationToken cancellationToken);
}
