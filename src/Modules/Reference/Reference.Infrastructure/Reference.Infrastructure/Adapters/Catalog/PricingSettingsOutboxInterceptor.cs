using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Reference.Domain.PpeMarkupRules;
using Reference.Domain.PricingParameters;

namespace Reference.Infrastructure.Adapters.Catalog;

/// <summary>
/// Adds the <c>reference.pricing-settings.changed</c> outbox message to every save that adds, changes or deletes a
/// pricing parameter or a PPE markup rule, so Catalog learns about it even if the request stops (PLT-003 R-09).
/// </summary>
internal sealed class PricingSettingsOutboxInterceptor(TimeProvider clock) : SaveChangesInterceptor
{
    private readonly List<Guid> _pending = [];
    private readonly List<Guid> _saved = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Enqueue(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Enqueue(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Saved();
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Saved();
        return base.SavedChanges(eventData, result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData) => _pending.Clear();

    public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _pending.Clear();
        return base.SaveChangesFailedAsync(eventData, cancellationToken);
    }

    /// <summary>The ids of the messages saved since the last call.</summary>
    public IReadOnlyCollection<Guid> TakeSaved()
    {
        var ids = _saved.ToArray();
        _saved.Clear();
        return ids;
    }

    private void Enqueue(DbContext? context)
    {
        if (context is null || !context.ChangeTracker.Entries().Any(entry =>
                entry is { Entity: PricingParameter or PpeMarkupRule, State: EntityState.Added or EntityState.Modified or EntityState.Deleted }))
            return;
        var message = OutboxMessage.Create(PricingSettingsChangedOutboxHandler.MessageType,
            new PricingSettingsChangedPayload(), clock.GetUtcNow());
        context.Add(message);
        _pending.Add(message.Id);
    }

    private void Saved()
    {
        _saved.AddRange(_pending);
        _pending.Clear();
    }
}
