using BuildingBlocks.Application.Actors;
using BuildingBlocks.Application.DomainEvents;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace BuildingBlocks.Infrastructure.DomainEvents;

/// <summary>
/// Writes the domain events of a module's save to its outbox in the same transaction (ADR-0016).
/// <list type="number">
/// <item>Before the save: takes the events of the tracked entities, clears them and, when the caller has no
/// transaction, opens one.</item>
/// <item>After the save (database ids are known now): the modules' <see cref="IDomainEventOutbox"/> translations turn
/// the events into outbox messages, a second save writes them, and the opened transaction commits.</item>
/// </list>
/// A failure of either save rolls the opened transaction back, so neither the change nor its messages stay. The ids of
/// the saved messages go to <see cref="ISavedOutboxMessages"/> for the try after the command.
/// </summary>
public sealed class DomainEventOutboxInterceptor(
    IEnumerable<IDomainEventOutbox> outboxes,
    ICurrentActor actor,
    ISavedOutboxMessages saved,
    IDomainEventSuppression suppression,
    TimeProvider clock) : SaveChangesInterceptor
{
    private readonly IDomainEventOutbox[] _outboxes = outboxes.ToArray();
    private readonly List<IDomainEvent> _events = [];
    private IDbContextTransaction? _transaction;
    private bool _writing;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (!_writing && Take(eventData.Context) is { } context)
            _transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return result;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (!_writing && Take(eventData.Context) is { } context) _transaction = context.Database.BeginTransaction();
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_writing || eventData.Context is not { } context) return result;
        try
        {
            var messages = Describe();
            if (messages.Count > 0)
            {
                context.AddRange(messages);
                _writing = true;
                await context.SaveChangesAsync(cancellationToken);
            }

            if (_transaction is not null) await _transaction.CommitAsync(cancellationToken);
            saved.Add(messages.Select(x => x.Id));
        }
        catch
        {
            if (_transaction is not null) await _transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            await EndAsync();
        }

        return result;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_writing || eventData.Context is not { } context) return result;
        try
        {
            var messages = Describe();
            if (messages.Count > 0)
            {
                context.AddRange(messages);
                _writing = true;
                context.SaveChanges();
            }

            _transaction?.Commit();
            saved.Add(messages.Select(x => x.Id));
        }
        catch
        {
            _transaction?.Rollback();
            throw;
        }
        finally
        {
            End();
        }

        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (_writing) return;
        _transaction?.Rollback();
        End();
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (_writing) return;
        if (_transaction is not null) await _transaction.RollbackAsync(CancellationToken.None);
        await EndAsync();
    }

    /// <summary>
    /// Moves the events of the tracked entities into this save. Returns the context when it needs its own
    /// transaction (there are events and the caller opened none).
    /// </summary>
    private DbContext? Take(DbContext? context)
    {
        if (context is null) return null;
        foreach (var source in context.ChangeTracker.Entries<IHasDomainEvents>().Select(x => x.Entity).ToArray())
        {
            if (source.DomainEvents.Count == 0) continue;
            if (!suppression.IsSuppressed) _events.AddRange(source.DomainEvents);
            source.ClearDomainEvents();
        }

        return _events.Count > 0 && _transaction is null && context.Database.CurrentTransaction is null ? context : null;
    }

    private List<OutboxMessage> Describe()
    {
        if (_events.Count == 0) return [];
        var who = actor.Current;
        var now = clock.GetUtcNow();
        return _events.SelectMany(domainEvent => _outboxes.SelectMany(outbox => outbox.Write(domainEvent, who, now)))
            .ToList();
    }

    private void End()
    {
        _transaction?.Dispose();
        Reset();
    }

    private async ValueTask EndAsync()
    {
        if (_transaction is not null) await _transaction.DisposeAsync();
        Reset();
    }

    private void Reset()
    {
        _transaction = null;
        _events.Clear();
        _writing = false;
    }
}
