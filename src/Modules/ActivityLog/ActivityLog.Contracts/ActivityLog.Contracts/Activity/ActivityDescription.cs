namespace ActivityLog.Contracts.Activity;

/// <summary>What happened to which object, in the owner's words (ACT-001 R-02, R-03).</summary>
/// <param name="Action">One of <see cref="ActivityActions"/>.</param>
/// <param name="SubjectType">A stable kebab-case key such as <c>quote</c>.</param>
/// <param name="SubjectTypeTitle">Its Ukrainian name such as «КП».</param>
/// <param name="SubjectId">The object's id as text.</param>
/// <param name="SubjectTitle">Its number or name.</param>
/// <param name="Summary">A short note; null when the changes say it all.</param>
/// <param name="Changes">Changed fields with display values; empty for a create or delete.</param>
/// <param name="SourceModule">The module, such as <c>Crm</c>.</param>
public sealed record ActivityDescription(
    string Action,
    string SubjectType,
    string SubjectTypeTitle,
    string SubjectId,
    string SubjectTitle,
    string? Summary,
    IReadOnlyList<ActivityChangeItem> Changes,
    string SourceModule);
