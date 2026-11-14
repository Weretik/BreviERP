namespace Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;

/// <summary>A column of the board: the orders whose slowest item is at this stage.</summary>
public sealed record DashboardStageResponse(int Stage, string Label, IReadOnlyList<DashboardOrderResponse> Orders);
