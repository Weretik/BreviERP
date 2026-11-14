using Production.Application.Abstractions;
using Production.Domain.Timesheet;
using Production.Application.Features.Orders.Shared.Validation;

namespace Production.Application.Features.Timesheet.Queries.GetTimesheetDay;

public sealed class GetTimesheetDayValidator : AbstractValidator<GetTimesheetDayQuery>
{
    public GetTimesheetDayValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Date).NotEmpty();
    }
}
