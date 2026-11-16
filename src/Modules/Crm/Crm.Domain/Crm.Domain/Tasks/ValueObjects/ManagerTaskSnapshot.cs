namespace Crm.Domain.Tasks.ValueObjects;

/// <summary>The fields of a task an edit can change, kept to show what changed in «Журнал дій» (ACT-001).</summary>
public sealed record ManagerTaskSnapshot(
    string Title,
    string? Description,
    string AssigneeName,
    DateTimeOffset DueAt,
    bool IsUrgent,
    string? CustomerName,
    int StepCount);
