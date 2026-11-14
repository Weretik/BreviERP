namespace Crm.Domain.Tasks.Enums;

/// <summary>The kanban column of a task at a moment (R-06): Overdue wins over Urgent.</summary>
public enum ManagerTaskLane
{
    Overdue,
    Urgent,
    Active,
    Done,
}
