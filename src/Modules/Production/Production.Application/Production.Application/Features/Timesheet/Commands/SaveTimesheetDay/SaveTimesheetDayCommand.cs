using Production.Application.Abstractions;
using Production.Domain.Timesheet;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Timesheet.Shared;

namespace Production.Application.Features.Timesheet.Commands.SaveTimesheetDay;

/// <summary>«Зберегти день»: the marks of the changed workers, each change kept in the history.</summary>
public sealed record SaveTimesheetDayCommand(ProductionActor Actor, SaveTimesheetDayInput Input)
    : ICommand<Result<TimesheetDayResponse>>;
