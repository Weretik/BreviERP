namespace Crm.Domain.Tasks.ValueObjects;

/// <summary>Everything Director or Admin sets on a task (R-02, R-03).</summary>
public sealed record ManagerTaskDraft(
    string Title,
    string? Description,
    ManagerTaskPerson Assignee,
    DateTimeOffset DueAt,
    bool IsUrgent,
    Guid? CustomerPublicId,
    string? CustomerName,
    IReadOnlyList<ManagerTaskStepDraft> Steps);

public sealed record ManagerTaskStepDraft(string Text, bool IsDone);
