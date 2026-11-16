namespace ActivityLog.Application.Features.Entries.Queries.GetActivityEntries;

/// <summary>One row of «Журнал дій» (ACT-001 R-09).</summary>
public sealed record ActivityEntryDto(
    Guid Id,
    DateTimeOffset OccurredAt,
    int ActorId,
    string ActorName,
    IReadOnlyList<string> ActorRoles,
    string Action,
    string SubjectType,
    string SubjectTypeTitle,
    string SubjectId,
    string SubjectTitle,
    string? Summary,
    IReadOnlyList<ActivityChangeDto> Changes);
