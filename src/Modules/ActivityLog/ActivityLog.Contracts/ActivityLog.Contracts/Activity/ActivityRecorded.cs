namespace ActivityLog.Contracts.Activity;

/// <summary>
/// A staff member's action for «Журнал дій» (ACT-001). A module writes it as the outbox message
/// <see cref="ActivityMessage.Type"/> in the same save as the action; ActivityLog stores it once under
/// <see cref="EventId"/>.
/// </summary>
/// <param name="EventId">A new Guid v7 per action; a repeat of the message carries the same id.</param>
/// <param name="ActorId">The staff account id.</param>
/// <param name="ActorLogin">E-mail or user name at the moment of the action, shown when the account has no name.</param>
/// <param name="ActorRoles">Role names at the moment of the action.</param>
/// <param name="Action">One of <see cref="ActivityActions"/>.</param>
/// <param name="SubjectType">A stable kebab-case key such as <c>quote</c>.</param>
/// <param name="SubjectTypeTitle">Its Ukrainian name such as «КП».</param>
/// <param name="SubjectId">The id of the object as text.</param>
/// <param name="SubjectTitle">Its number or name such as «№125».</param>
/// <param name="Summary">A short note such as «Статус: В роботі»; null when the changes say it all.</param>
/// <param name="Changes">The fields the person changed, with display values.</param>
/// <param name="SourceModule">The module that raised it, such as <c>Crm</c>.</param>
public sealed record ActivityRecorded(
    Guid EventId,
    DateTimeOffset OccurredAt,
    int ActorId,
    string ActorLogin,
    IReadOnlyList<string> ActorRoles,
    string Action,
    string SubjectType,
    string SubjectTypeTitle,
    string SubjectId,
    string SubjectTitle,
    string? Summary,
    IReadOnlyList<ActivityChangeItem> Changes,
    string SourceModule);

/// <summary>One changed field: its Ukrainian label and the old and new values as the admin shows them.</summary>
public sealed record ActivityChangeItem(string Field, string? From, string? To);
