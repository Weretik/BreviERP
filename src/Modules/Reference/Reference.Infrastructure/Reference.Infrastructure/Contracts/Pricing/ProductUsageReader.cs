using Reference.Application.Abstractions;
using Reference.Application.Abstractions.Pricing;
using Reference.Contracts.Pricing;

namespace Reference.Infrastructure.Contracts.Pricing;

/// <summary>
/// Before a delete: a Reference row is in use when a pricing version keeps it or a record of another module holds it
/// (every registered <see cref="IPricingInputUsageSource"/>, ADR-0019).
/// </summary>
internal sealed class ProductUsageReader(IReferencePricingUsageReader pricingUsage,
    IEnumerable<IPricingInputUsageSource> sources) : IProductUsageReader
{
    public async Task<bool> IsFabricUsedAsync(int fabricId, CancellationToken cancellationToken) =>
        await pricingUsage.IsFabricVersionedAsync(fabricId, cancellationToken) ||
        await AnyAsync(source => source.IsFabricUsedAsync(fabricId, cancellationToken));

    public async Task<bool> IsGarmentAccessoryUsedAsync(int accessoryId, CancellationToken cancellationToken) =>
        await pricingUsage.IsAccessoryVersionedAsync(accessoryId, cancellationToken) ||
        await AnyAsync(source => source.IsAccessoryUsedAsync(accessoryId, cancellationToken));

    public async Task<bool> IsGarmentPartOperationUsedAsync(int operationId, CancellationToken cancellationToken) =>
        await pricingUsage.IsOperationVersionedAsync(operationId, cancellationToken) ||
        await AnyAsync(source => source.IsOperationUsedAsync(operationId, cancellationToken));

    private async Task<bool> AnyAsync(Func<IPricingInputUsageSource, Task<bool>> used)
    {
        foreach (var source in sources)
            if (await used(source))
                return true;
        return false;
    }
}
