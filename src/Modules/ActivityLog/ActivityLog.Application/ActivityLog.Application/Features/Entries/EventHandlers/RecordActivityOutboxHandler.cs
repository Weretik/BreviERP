using BuildingBlocks.Application.Idempotency;
using ActivityLog.Domain.Filters;
using ActivityLog.Domain.Entries;
using ActivityLog.Contracts.Activity;
using BuildingBlocks.Application.Outbox;

namespace ActivityLog.Application.Features.Entries.EventHandlers;

/// <summary>
/// Stores an <see cref="ActivityRecorded"/> that another module wrote to its outbox (ACT-001 R-05). The entry id is the
/// event id: a repeat finds the row and does nothing, and a racing copy is refused by the primary key and counts as done.
/// The person and the object type of the entry go to the filter tables in the same save. Two first entries of the same
/// person at once: the loser's save is refused and the outbox retries it, then it finds the row.
/// </summary>
public sealed class RecordActivityOutboxHandler(IActivityLogDbContext db, IActorNames names, TimeProvider clock)
    : IOutboxHandler
{
    public string Type => ActivityMessage.Type;

    public async Task<string?> HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var activity = OutboxMessage.Read<ActivityRecorded>(payload);
        if (await db.Entries.AsNoTracking().AnyAsync(x => x.Id == new ActivityEntryId(activity.EventId), cancellationToken)) return null;

        var name = await names.FindAsync(activity.ActorId, cancellationToken);
        var entry = ActivityEntry.Create(activity.EventId, activity.OccurredAt, activity.ActorId,
            string.IsNullOrWhiteSpace(name) ? activity.ActorLogin : name, activity.ActorRoles, activity.Action,
            activity.SubjectType, activity.SubjectTypeTitle, activity.SubjectId, activity.SubjectTitle, activity.Summary,
            activity.Changes.Select(x => ActivityChange.Of(x.Field, x.From, x.To)), activity.SourceModule,
            clock.GetUtcNow());
        db.Entries.Add(entry);
        await NoteFiltersAsync(entry, cancellationToken);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsUniqueViolation())
        {
            // Another run stored the same event first: done. Otherwise a filter row raced: the outbox retries.
            if (!await db.Entries.AsNoTracking().AnyAsync(x => x.Id == new ActivityEntryId(activity.EventId), cancellationToken)) throw;
        }

        return null;
    }

    private async Task NoteFiltersAsync(ActivityEntry entry, CancellationToken cancellationToken)
    {
        var actor = await db.Actors.FirstOrDefaultAsync(x => x.Id == entry.ActorId, cancellationToken);
        if (actor is null) db.Actors.Add(ActivityActor.Create(entry.ActorId.Value, entry.ActorName, entry.OccurredAt));
        else actor.Seen(entry.ActorName, entry.OccurredAt);

        var type = await db.SubjectTypes.FirstOrDefaultAsync(x => x.Key == entry.SubjectType, cancellationToken);
        if (type is null) db.SubjectTypes.Add(ActivitySubjectType.Create(entry.SubjectType, entry.SubjectTypeTitle, entry.OccurredAt));
        else type.Seen(entry.SubjectTypeTitle, entry.OccurredAt);
    }
}
