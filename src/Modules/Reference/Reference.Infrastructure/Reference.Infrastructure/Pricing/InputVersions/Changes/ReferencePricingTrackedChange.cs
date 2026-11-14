using System.Linq.Expressions;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Pricing;

namespace Reference.Infrastructure.Pricing.InputVersions.Changes;

/// <summary>
/// The pricing state a Fabric, GarmentAccessory or GarmentPartOperation will have after the save.
/// <see cref="OriginalValue"/> is null for a new row; <see cref="Deleted"/> rows lose their versions.
/// </summary>
internal sealed record ReferencePricingTrackedChange(
    string Key,
    FabricId? FabricId,
    GarmentAccessoryId? AccessoryId,
    GarmentPartOperationId? OperationId,
    decimal Value,
    decimal? OriginalValue,
    PricingInputUnit Unit,
    bool IsActive,
    bool Deleted)
{
    public Expression<Func<ReferencePricingInputVersion, bool>> VersionFilter =>
        FabricId.HasValue ? x => x.FabricId == FabricId
        : AccessoryId.HasValue ? x => x.AccessoryId == AccessoryId
        : x => x.OperationId == OperationId;
}
