using Production.Application.Features.Orders.Shared.Validation;
namespace Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard.Results;

/// <summary>An open order on the board; <c>Progress</c> is the share of pieces sewn within the plan, 0–100.</summary>
public sealed record DashboardOrderResponse(int Id, int Number, string ClientName, string Product, int Items,
    int Quantity, int Progress, DateOnly DueDate, bool IsLate);
