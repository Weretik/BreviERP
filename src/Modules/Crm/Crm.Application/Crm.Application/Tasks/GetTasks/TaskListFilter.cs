namespace Crm.Application.Tasks.GetTasks;

/// <summary>Filter chips of «Завдання менеджерам» (R-09).</summary>
public enum TaskListFilter
{
    Open,
    Overdue,
    Urgent,
    New,
    InProgress,
    Done,
}
