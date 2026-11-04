using Production.Application.Contracts;

namespace Production.Application.Staff.GetStaff;

/// <summary>Employees by name: active ones, or also the archived when <paramref name="IncludeArchived"/>.</summary>
public sealed record GetWorkshopStaffQuery(bool IncludeArchived) : IQuery<Result<IReadOnlyList<WorkshopEmployeeResponse>>>;
