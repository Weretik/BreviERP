using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Reports.Shared;

namespace Production.Application.Features.Reports.Queries.GetWorkshopReport;

/// <summary>
/// «Звіти цеху» of the months <c>From</c>..<c>To</c> («РРРР-ММ», at most 12, not in the future) and, when both are
/// given, of the months to compare with (PRD-D48).
/// </summary>
public sealed record GetWorkshopReportQuery(ProductionActor Actor, string From, string To, string? CompareFrom,
    string? CompareTo) : IQuery<Result<WorkshopReportPairResponse>>;
