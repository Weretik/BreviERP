using Crm.Domain.Tasks.Enums;
using Crm.Domain.Tasks.ValueObjects;

namespace Crm.Domain.Tasks;

/// <summary>
/// A task was created or changed (ACT-001). It carries the task itself, because a new task gets its id
/// only when it is saved; <paramref name="Before"/> is set for an edit.
/// </summary>
public sealed record ManagerTaskRecorded(ManagerTask Task, ManagerTaskEventKind Kind, string? Details,
    ManagerTaskSnapshot? Before) : IDomainEvent;

/// <summary>A task was deleted (ACT-001).</summary>
public sealed record ManagerTaskDeleted(long TaskId, string Title) : IDomainEvent;
