using Microsoft.EntityFrameworkCore.Diagnostics;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.GarmentPartOperations;
using Reference.Domain.Pricing;
using Reference.Domain.Fabrics;
using Reference.Contracts.Pricing;

namespace Reference.Infrastructure.Persistence;

/// <summary>
/// Tells every <see cref="IPricingInputSubscriber"/> when a save adds a pricing input version (a new or changed fabric,
/// accessory or operation price) or deletes one of those rows, so Catalog publishes the new prices (CAT-012,
/// ADR-0019). A rename or another change without a new version tells nothing.
/// </summary>
internal sealed class ReferencePricingInputChangeInterceptor(IEnumerable<IPricingInputSubscriber> subscribers) : SaveChangesInterceptor
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
            foreach (var subscriber in subscribers)
                subscriber.PricingInputsChanged();
        _changed = false;
    }
}
