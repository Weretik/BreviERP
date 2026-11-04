namespace Crm.Domain.Tasks;

/// <summary>Length and count limits of a manager task (TSK-001 R-03).</summary>
public static class ManagerTaskLimits
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 4000;
    public const int NameMaxLength = 200;
    public const int StepTextMaxLength = 300;
    public const int MaxSteps = 30;
    public const int DetailsMaxLength = 400;
}
