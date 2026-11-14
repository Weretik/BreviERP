namespace Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard.Results;

public sealed record DashboardHoursResponse(int EmployeeId, string Name, int Minutes);
