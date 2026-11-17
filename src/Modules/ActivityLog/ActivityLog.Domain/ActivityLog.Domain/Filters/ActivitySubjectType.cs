using BuildingBlocks.Domain.Entity;
using ActivityLog.Domain.Entries;

namespace ActivityLog.Domain.Filters;

/// <summary>
/// An object type in the «Об'єкт» filter of «Журнал дій» with the title of its latest entry. Kept by the recording
/// handler, so the filters never group the whole journal (database rules 2.2).
/// </summary>
public sealed class ActivitySubjectType : IAggregateRoot
{
    private ActivitySubjectType() { }

    public string Key { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public DateTimeOffset LastSeenAt { get; private set; }

    public static ActivitySubjectType Create(string key, string title, DateTimeOffset seenAt) => new()
    {
        Key = ActivityText.Cut(key, ActivityEntry.SubjectTypeMaxLength),
        Title = ActivityText.Cut(title, ActivityEntry.SubjectTypeTitleMaxLength),
        LastSeenAt = seenAt,
    };

    /// <summary>A later entry brings its title; an older one changes nothing.</summary>
    public void Seen(string title, DateTimeOffset at)
    {
        if (at < LastSeenAt) return;
        Title = ActivityText.Cut(title, ActivityEntry.SubjectTypeTitleMaxLength);
        LastSeenAt = at;
    }
}
