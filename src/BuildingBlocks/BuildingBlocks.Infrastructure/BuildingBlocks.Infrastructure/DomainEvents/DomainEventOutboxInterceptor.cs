using BuildingBlocks.Application.Actors;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BuildingBlocks.Infrastructure.DomainEvents;

/// <summary>
/// On every save of a module, takes the domain events of the tracked entities, turns them into outbox messages through
/// the modules' <see cref="IDomainEventOutbox"/> translations and adds them to the same save (ADR-0016). The events
/// are cleared; the ids of the saved messages go to <see cref="ISavedOutboxMessages"/> for the try after the command.
/// A failed save writes and remembers nothing.
/// </summary>
public sealed class DomainEventOutboxInterceptor(
    IEnumerable<IDomainEventOutbox> outboxes,
    ICurrentActor actor,
    ISavedOutboxMessages saved,
    TimeProvider clock) : SaveChangesInterceptor
{
    private readonly IDomainEventOutbox[] _outboxes = outboxes.ToArray();
    private readonly List<Guid> _pending = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Write(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Write(eventData.Context);
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

    private void Write(DbContext? context)
    {
        if (context is null) return;
        var sources = context.ChangeTracker.Entries<IHasDomainEvents>()
            .Select(entry => entry.Entity)
            .Where(entity => entity.DomainEvents.Count > 0)
            .ToArray();
        if (sources.Length == 0) return;

        var who = actor.Current;
        var now = clock.GetUtcNow();
        foreach (var source in sources)
        {
            foreach (var domainEvent in source.DomainEvents)
            foreach (var outbox in _outboxes)
            foreach (var message in outbox.Write(domainEvent, who, now))
            {
                context.Add(message);
                _pending.Add(message.Id);
            }

            source.ClearDomainEvents();
        }
    }

    private void Saved()
    {
        saved.Add(_pending);
        _pending.Clear();
    }
}
