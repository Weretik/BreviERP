using Catalog.Contracts.Pricing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Pricing;

namespace Reference.Infrastructure.Pricing.InputVersions.Changes;

/// <summary>
/// Marks <see cref="PricingInputChanges"/> for every Sewing model when a save adds a pricing input version (a new or
/// changed fabric, accessory or operation price) or deletes one of those rows, so the host publishes the new prices
/// (CAT-012). A rename or another change without a new version marks nothing.
/// </summary>
internal sealed class ReferencePricingInputChangeInterceptor(PricingInputChanges changes) : SaveChangesInterceptor
{
    private bool _changed;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        _changed = eventData.Context is { } context && Changed(context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        _changed = eventData.Context is { } context && Changed(context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Flush();
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Flush();
        return base.SavedChanges(eventData, result);
    }

    private static bool Changed(DbContext context) => context.ChangeTracker.Entries().Any(entry =>
        entry is { Entity: ReferencePricingInputVersion, State: EntityState.Added }
        || entry is { Entity: Fabric or GarmentAccessory or GarmentPartOperation, State: EntityState.Deleted });

    private void Flush()
    {
        if (_changed)
            changes.MarkAll();
        _changed = false;
    }
}
