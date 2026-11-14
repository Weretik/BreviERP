namespace Crm.Domain.Tasks.Enums;

/// <summary>What the manager did with the task. «Прострочено» and «Терміново» are not statuses (R-05).</summary>
public enum ManagerTaskStatus
{
    New,
    InProgress,
    Done,
}
