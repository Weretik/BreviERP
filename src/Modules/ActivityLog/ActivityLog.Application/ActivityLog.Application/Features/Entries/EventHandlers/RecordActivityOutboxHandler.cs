using ActivityLog.Contracts.Activity;
using BuildingBlocks.Application.Outbox;

namespace ActivityLog.Application.Features.Entries.EventHandlers;

/// <summary>
/// Stores an <see cref="ActivityRecorded"/> that another module wrote to its outbox (ACT-001 R-05). The entry id is the
/// event id: a repeat finds the row and does nothing, and a racing copy is refused by the primary key and counts as done.
/// </summary>
public sealed class RecordActivityOutboxHandler(IActivityLogDbContext db, IActorNames names, TimeProvider clock)
    : IOutboxHandler
{
    public string Type => ActivityMessage.Type;

    public async Task<string?> HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var activity = OutboxMessage.Read<ActivityRecorded>(payload);
        if (await db.Entries.AsNoTracking().AnyAsync(x => x.Id == activity.EventId, cancellationToken)) return null;

        var name = await names.FindAsync(activity.ActorId, cancellationToken);
        var entry = ActivityEntry.Create(activity.EventId, activity.OccurredAt, activity.ActorId,
            string.IsNullOrWhiteSpace(name) ? activity.ActorLogin : name, activity.ActorRoles, activity.Action,
            activity.SubjectType, activity.SubjectTypeTitle, activity.SubjectId, activity.SubjectTitle, activity.Summary,
            activity.Changes.Select(x => ActivityChange.Of(x.Field, x.From, x.To)), activity.SourceModule,
            clock.GetUtcNow());
        db.Entries.Add(entry);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.IsUniqueViolation())
        {
            // Another run stored the same event first.
        }

        return null;
    }
}
