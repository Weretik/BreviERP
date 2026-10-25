using Microsoft.EntityFrameworkCore.ChangeTracking;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;

namespace Reference.Infrastructure.Pricing.InputVersions.Changes;

internal static class TrackedReferencePricingChanges
{
    public static bool Any(ChangeTracker tracker)
    {
        tracker.DetectChanges();
        return tracker.Entries().Any(entry => entry.State == EntityState.Modified
            && (entry.Entity is Fabric && entry.Property(nameof(Fabric.Price)).IsModified
                || entry.Entity is GarmentAccessory && entry.Property(nameof(GarmentAccessory.Price)).IsModified
                || entry.Entity is GarmentPartOperation && entry.Property(nameof(GarmentPartOperation.Min)).IsModified));
    }

    public static IReadOnlyList<ReferencePricingTrackedChange> Read(ChangeTracker tracker)
    {
        var changes = new List<ReferencePricingTrackedChange>();
        foreach (var entry in tracker.Entries().Where(x => x.State == EntityState.Modified))
        {
            ReferencePricingTrackedChange? change = null;
            if (entry.Entity is Fabric fabric && entry.Property(nameof(Fabric.Price)).IsModified)
                change = new(x => x.FabricId == fabric.Id, fabric.Price.Value,
                    ((MoneyAmount)entry.Property(nameof(Fabric.Price)).OriginalValue!).Value);
            else if (entry.Entity is GarmentAccessory accessory && entry.Property(nameof(GarmentAccessory.Price)).IsModified)
                change = new(x => x.AccessoryId == accessory.Id, accessory.Price.Value,
                    ((MoneyAmount)entry.Property(nameof(GarmentAccessory.Price)).OriginalValue!).Value);
            else if (entry.Entity is GarmentPartOperation operation && entry.Property(nameof(GarmentPartOperation.Min)).IsModified)
                change = new(x => x.OperationId == operation.Id, operation.Min,
                    (decimal)entry.Property(nameof(GarmentPartOperation.Min)).OriginalValue!);
            if (change is not null && change.Value != change.OriginalValue)
                changes.Add(change);
        }
        return changes;
    }
}
