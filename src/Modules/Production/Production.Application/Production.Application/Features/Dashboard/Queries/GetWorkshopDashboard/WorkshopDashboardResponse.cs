namespace Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;

/// <summary>
/// «Огляд виробництва» on the admin home (PRD-D50): open orders by stage, late ones and what is ready to ship.
/// <c>Hours</c> is the hours of this week by worker, only for those who keep «Робочий час».
/// </summary>
public sealed record WorkshopDashboardResponse(
    DateOnly Today,
    int OpenOrders,
    int OpenPieces,
    int LateOrders,
    int ReadyOrders,
    int ReadyPieces,
    IReadOnlyList<DashboardStageResponse> Stages,
    IReadOnlyList<DashboardHoursResponse>? Hours);
