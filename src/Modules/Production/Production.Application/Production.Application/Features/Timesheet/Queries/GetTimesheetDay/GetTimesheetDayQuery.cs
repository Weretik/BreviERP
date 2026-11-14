using Production.Application.Abstractions;
using Production.Domain.Timesheet;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Timesheet.Shared;

namespace Production.Application.Features.Timesheet.Queries.GetTimesheetDay;

/// <summary>«Робочий час» of a day (PRD-D41): active workers and anyone marked that day.</summary>
public sealed record GetTimesheetDayQuery(ProductionActor Actor, DateOnly Date) : IQuery<Result<TimesheetDayResponse>>;
