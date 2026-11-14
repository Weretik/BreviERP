using Production.Application.Abstractions;
using Production.Domain.Timesheet;
using Production.Application.Features.Timesheet.Shared;

namespace Production.Application.Features.Timesheet.Queries.GetTimesheetDay;

public sealed class GetTimesheetDayHandler(TimesheetDays days)
    : IQueryHandler<GetTimesheetDayQuery, Result<TimesheetDayResponse>>
{
    public async ValueTask<Result<TimesheetDayResponse>> Handle(GetTimesheetDayQuery query,
        CancellationToken cancellationToken) =>
        query.Actor.CanKeepTime
            ? Result.Success(await days.BuildAsync(query.Date, cancellationToken))
            : Result<TimesheetDayResponse>.Forbidden(TimesheetMessages.CannotSee);
}
