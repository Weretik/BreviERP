using Production.Application.Abstractions;
using Production.Domain.Timesheet;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Timesheet.Shared;
using Production.Application.Features.Work.Shared;
using BuildingBlocks.Application;

namespace Production.Application.Features.Timesheet.Commands.SaveTimesheetDay;

public sealed class SaveTimesheetDayHandler(IProductionDbContext db, IProductionStaffReader staff,
    TimesheetDays days, TimeProvider clock) : ICommandHandler<SaveTimesheetDayCommand, Result<TimesheetDayResponse>>
{
    public async ValueTask<Result<TimesheetDayResponse>> Handle(SaveTimesheetDayCommand command,
        CancellationToken cancellationToken)
    {
        var input = command.Input;
        if (!command.Actor.CanKeepTime) return Result<TimesheetDayResponse>.Forbidden(TimesheetMessages.CannotSee);
        if (input.Date > WorkshopDay.Today(clock))
            return Result<TimesheetDayResponse>.Invalid(new ValidationError("Input.Date", TimesheetMessages.FutureDate));
        if (await PayrollMonths.IsClosedAsync(db, input.Date, cancellationToken))
            return Result<TimesheetDayResponse>.Conflict(PayrollMessages.Closed);
        var ids = input.Entries.Select(x => x.EmployeeId).ToList();
        var employees = await db.Employees.AsNoTracking().Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        if (employees.Count != ids.Count)
            return Result<TimesheetDayResponse>.Invalid(new ValidationError("Input.Entries", TimesheetMessages.EmployeeNotFound));
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<TimesheetDayResponse>.Forbidden(TimesheetMessages.CannotSee);
        var existing = await db.TimeEntries.Where(x => x.WorkDate == input.Date && ids.Contains(x.EmployeeId))
            .ToDictionaryAsync(x => x.EmployeeId, cancellationToken);

        var now = clock.GetUtcNow();
        foreach (var entry in input.Entries)
        {
            var row = existing.GetValueOrDefault(entry.EmployeeId);
            if (row?.Version != entry.Version) return Result<TimesheetDayResponse>.Conflict(TimesheetMessages.Stale);
            if (row?.Minutes == entry.Minutes) continue;
            var name = employees[entry.EmployeeId].FullName;
            db.TimeChanges.Add(new WorkshopTimeChange(entry.EmployeeId, name, input.Date, row?.Minutes, entry.Minutes,
                actorName, now));
            if (row is null)
            {
                var created = WorkshopTimeEntry.Create(entry.EmployeeId, input.Date, entry.Minutes, actorName, now);
                if (created.IsFailure) return created.Error!.ToResult<TimesheetDayResponse>();
                db.TimeEntries.Add(created.Value);
            }
            else if (row.Change(entry.Minutes, actorName, now) is { IsFailure: true } changed)
                return changed.Error!.ToResult<TimesheetDayResponse>();
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await days.BuildAsync(input.Date, cancellationToken));
    }
}
