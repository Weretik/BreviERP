using Production.Application.Abstractions;
using Production.Domain.Orders;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Dashboard.Queries.GetWorkshopDashboard;

/// <summary>
/// «Огляд виробництва» on the admin home (PRD-D50): the «Нове» orders on a board by the stage of their slowest item,
/// how many are late and what is ready to ship. Everyone with production orders sees it; no prices or rates.
/// </summary>
public sealed record GetWorkshopDashboardQuery(ProductionActor Actor) : IQuery<Result<WorkshopDashboardResponse>>;
