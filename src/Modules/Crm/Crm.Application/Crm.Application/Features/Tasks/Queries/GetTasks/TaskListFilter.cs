namespace Crm.Application.Features.Tasks.Queries.GetTasks;

/// <summary>Filter chips of «Завдання менеджерам» (R-09).</summary>
public enum TaskListFilter
{
    Open,
    Overdue,
    Urgent,
    New,
    InProgress,
    Done,

    /// <summary>Open and done together: the client card tab «Завдання менеджерам» (CARD-001 R-05).</summary>
    All,
}
