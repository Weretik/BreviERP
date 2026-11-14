using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Queries.GetWorkshopStaff;

/// <summary>Employees by name: active ones, or also the archived when <paramref name="IncludeArchived"/>.</summary>
public sealed record GetWorkshopStaffQuery(bool IncludeArchived) : IQuery<Result<IReadOnlyList<WorkshopEmployeeResponse>>>;
