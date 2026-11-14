namespace Production.Application.Features.Work.Queries.GetWorkChoices;

/// <summary>What the forms of «Журнал робіт» pick from.</summary>
public sealed record WorkChoicesResponse(
    IReadOnlyList<WorkEmployeeChoice> Employees,
    IReadOnlyList<WorkItemChoice> Items,
    IReadOnlyList<string> OverPlanReasons);
