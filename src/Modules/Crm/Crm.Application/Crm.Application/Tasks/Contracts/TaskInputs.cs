namespace Crm.Application.Tasks.Contracts;

/// <summary>The side panel of Director or Admin. <paramref name="Version"/> is 0 for a new task.</summary>
public sealed record SaveTaskInput(
    int Version,
    string Title,
    string? Description,
    int AssigneeUserId,
    DateTimeOffset DueAt,
    bool IsUrgent,
    Guid? CustomerPublicId,
    IReadOnlyList<TaskStepInput>? Steps);

public sealed record TaskStepInput(string Text, bool IsDone);
