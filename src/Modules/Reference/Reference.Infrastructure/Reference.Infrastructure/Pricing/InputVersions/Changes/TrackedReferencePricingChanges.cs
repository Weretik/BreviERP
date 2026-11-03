using Microsoft.EntityFrameworkCore.ChangeTracking;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Domain.Pricing;

namespace Reference.Infrastructure.Pricing.InputVersions.Changes;

/// <summary>
/// Added, modified and deleted pricing references in the change tracker. Every save of one keeps its
/// pricing input versions current: the first version for a new row, the next one for a changed price,
/// minutes, unit or activity.
/// </summary>
internal static class TrackedReferencePricingChanges
{
    private const string MetreUnit = "м";

    public static bool Any(ChangeTracker tracker)
    {
        tracker.DetectChanges();
        return tracker.Entries().Any(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
            && entry.Entity is Fabric or GarmentAccessory or GarmentPartOperation);
    }

    public static IReadOnlyList<ReferencePricingTrackedChange> Read(ChangeTracker tracker)
    {
        var changes = new List<ReferencePricingTrackedChange>();
        foreach (var entry in tracker.Entries()
                     .Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            var deleted = entry.State == EntityState.Deleted;
            var added = entry.State == EntityState.Added;
            ReferencePricingTrackedChange? change = entry.Entity switch
            {
                Fabric fabric => new("Fabric:" + fabric.Id.Value, fabric.Id, null, null, fabric.Price.Value,
                    added ? null : ((MoneyAmount)entry.Property(nameof(Fabric.Price)).OriginalValue!).Value,
                    PricingInputUnit.UahPerMetre, fabric.IsActive, deleted),
                GarmentAccessory accessory => new("Accessory:" + accessory.Id.Value, null, accessory.Id, null,
                    accessory.Price.Value,
                    added ? null : ((MoneyAmount)entry.Property(nameof(GarmentAccessory.Price)).OriginalValue!).Value,
                    AccessoryUnit(accessory.Unit), accessory.IsActive, deleted),
                GarmentPartOperation operation => new("Operation:" + operation.Id.Value, null, null, operation.Id,
                    operation.Min,
                    added ? null : (decimal)entry.Property(nameof(GarmentPartOperation.Min)).OriginalValue!,
                    PricingInputUnit.Minutes, operation.IsActive, deleted),
                _ => null
            };
            if (change is not null)
                changes.Add(change);
        }
        return changes;
    }

    /// <summary>A metre accessory is priced per metre; pieces and services are priced per piece.</summary>
    public static PricingInputUnit AccessoryUnit(string unit) =>
        unit == MetreUnit ? PricingInputUnit.UahPerMetre : PricingInputUnit.UahPerPiece;
}
