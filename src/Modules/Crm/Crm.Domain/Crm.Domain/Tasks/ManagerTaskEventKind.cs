namespace Crm.Domain.Tasks;

/// <summary>Kinds of «Історія змін» rows; the Admin turns them into Ukrainian text.</summary>
public enum ManagerTaskEventKind
{
    Created,
    Edited,
    Reassigned,
    DueChanged,
    StatusChanged,
    UrgentChanged,
    StepChecked,
    StepUnchecked,
}
