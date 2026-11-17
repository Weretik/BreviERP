using BuildingBlocks.Domain.Entity;
namespace ActivityLog.Domain.Entries;

/// <summary>
/// One action of a staff member in «Журнал дій» (ACT-001 R-02): who, what, with which object and when. Entries are
/// written once and never changed.
/// </summary>
public sealed class ActivityEntry : IAggregateRoot
{
    public const int ActorNameMaxLength = 200;
    public const int RolesMaxLength = 500;
    public const int ActionMaxLength = 32;
    public const int SubjectTypeMaxLength = 64;
    public const int SubjectTypeTitleMaxLength = 100;
    public const int SubjectIdMaxLength = 64;
    public const int SubjectTitleMaxLength = 200;
    public const int SummaryMaxLength = 500;
    public const int SourceModuleMaxLength = 32;
    public const int MaxChanges = 20;

    /// <summary>Roles are kept as <c>|Manager|Admin|</c>, so «has role» is one <c>LIKE '%|Manager|%'</c>.</summary>
    public const char RoleSeparator = '|';

    private ActivityEntry() { }

    public Guid Id { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public int ActorId { get; private set; }
    public string ActorName { get; private set; } = null!;
    public string ActorRoles { get; private set; } = null!;
    public string Action { get; private set; } = null!;
    public string SubjectType { get; private set; } = null!;
    public string SubjectTypeTitle { get; private set; } = null!;
    public string SubjectId { get; private set; } = null!;
    public string SubjectTitle { get; private set; } = null!;
    public string? Summary { get; private set; }
    public IReadOnlyList<ActivityChange> Changes { get; private set; } = [];
    public string SourceModule { get; private set; } = null!;
    public DateTimeOffset RecordedAt { get; private set; }

    public IReadOnlyList<string> RoleNames => RolesOf(ActorRoles);

    public static ActivityEntry Create(Guid id, DateTimeOffset occurredAt, int actorId, string actorName,
        IEnumerable<string> roles, string action, string subjectType, string subjectTypeTitle, string subjectId,
        string subjectTitle, string? summary, IEnumerable<ActivityChange> changes, string sourceModule,
        DateTimeOffset recordedAt)
    {
        if (id == Guid.Empty) throw new ArgumentException("An entry needs the event id.", nameof(id));
        return new ActivityEntry
        {
            Id = id,
            OccurredAt = occurredAt,
            ActorId = actorId,
            ActorName = ActivityText.Cut(actorName, ActorNameMaxLength),
            ActorRoles = JoinRoles(roles),
            Action = ActivityText.Cut(action, ActionMaxLength),
            SubjectType = ActivityText.Cut(subjectType, SubjectTypeMaxLength),
            SubjectTypeTitle = ActivityText.Cut(subjectTypeTitle, SubjectTypeTitleMaxLength),
            SubjectId = ActivityText.Cut(subjectId, SubjectIdMaxLength),
            SubjectTitle = ActivityText.Cut(subjectTitle, SubjectTitleMaxLength),
            Summary = ActivityText.CutOrNull(summary, SummaryMaxLength),
            Changes = changes.Take(MaxChanges).ToArray(),
            SourceModule = ActivityText.Cut(sourceModule, SourceModuleMaxLength),
            RecordedAt = recordedAt,
        };
    }

    /// <summary>The <c>LIKE</c> part that finds one role in <see cref="ActorRoles"/>.</summary>
    public static string RoleToken(string role) => $"{RoleSeparator}{role.Trim()}{RoleSeparator}";

    public static IReadOnlyList<string> RolesOf(string stored) =>
        stored.Split(RoleSeparator, StringSplitOptions.RemoveEmptyEntries);

    private static string JoinRoles(IEnumerable<string> roles)
    {
        var names = roles.Select(x => x.Trim()).Where(x => x.Length > 0 && !x.Contains(RoleSeparator))
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var joined = names.Length == 0 ? string.Empty : $"{RoleSeparator}{string.Join(RoleSeparator, names)}{RoleSeparator}";
        return joined.Length <= RolesMaxLength ? joined : joined[..joined.LastIndexOf(RoleSeparator, RolesMaxLength - 1)] + RoleSeparator;
    }
}
