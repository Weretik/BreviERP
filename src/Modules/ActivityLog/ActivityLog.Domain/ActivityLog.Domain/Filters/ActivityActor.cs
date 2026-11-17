using BuildingBlocks.Domain.Entity;
using ActivityLog.Domain.Entries;

namespace ActivityLog.Domain.Filters;

/// <summary>
/// A person in the «Хто» filter of «Журнал дій»: the Identity account id with the name of its latest entry. Kept by
/// the recording handler, so the filters never group the whole journal (database rules 2.2).
/// </summary>
public sealed class ActivityActor : IAggregateRoot
{
    private ActivityActor() { }

    public ActivityActorId Id { get; private set; }
    public string Name { get; private set; } = null!;
    public DateTimeOffset LastSeenAt { get; private set; }

    public static ActivityActor Create(int id, string name, DateTimeOffset seenAt) =>
        new() { Id = new ActivityActorId(id), Name = ActivityText.Cut(name, ActivityEntry.ActorNameMaxLength), LastSeenAt = seenAt };

    /// <summary>An entry of this person: a later one brings its name; an older one (a late outbox message) changes nothing.</summary>
    public void Seen(string name, DateTimeOffset at)
    {
        if (at < LastSeenAt) return;
        Name = ActivityText.Cut(name, ActivityEntry.ActorNameMaxLength);
        LastSeenAt = at;
    }
}
