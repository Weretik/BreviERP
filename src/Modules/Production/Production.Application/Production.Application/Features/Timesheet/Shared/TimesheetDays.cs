using Production.Application.Abstractions;
using Production.Domain.Timesheet;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Work.Shared;

namespace Production.Application.Features.Timesheet.Shared;

/// <summary>Builds a day of «Робочий час» with the totals of its month.</summary>
public sealed class TimesheetDays(IProductionDbContext db, TimeProvider clock)
{
    public async Task<TimesheetDayResponse> BuildAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var month = PayrollMonths.Of(date);
        var next = month.AddMonths(1);
        var employees = await db.Employees.AsNoTracking().ToDictionaryAsync(x => x.Id, cancellationToken);
        var entries = await db.TimeEntries.AsNoTracking().Where(x => x.WorkDate >= month && x.WorkDate < next)
            .ToListAsync(cancellationToken);
        var today = entries.Where(x => x.WorkDate == date).ToDictionary(x => x.EmployeeId);
        var changes = await db.TimeChanges.AsNoTracking().Where(x => x.WorkDate == date)
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var closed = await PayrollMonths.IsClosedAsync(db, date, cancellationToken);

        var rows = employees.Values.Where(x => x.IsActive || today.ContainsKey(x.Id))
            .OrderByDescending(x => x.IsActive).ThenBy(x => x.FullName, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => new TimesheetRowResponse(x.Id, x.FullName, x.Position, x.IsActive,
                today.GetValueOrDefault(x.Id)?.Minutes, today.GetValueOrDefault(x.Id)?.Version))
            .ToList();
        var byDay = entries.GroupBy(x => x.WorkDate).OrderBy(x => x.Key)
            .Select(x => new TimesheetMonthDayResponse(x.Key, x.Count(), x.Sum(e => e.Minutes))).ToList();
        var byEmployee = entries.GroupBy(x => x.EmployeeId)
            .Select(x =>
            {
                var employee = employees.GetValueOrDefault(x.Key);
                return new TimesheetMonthEmployeeResponse(x.Key, employee?.FullName ?? $"#{x.Key}",
                    employee?.Position ?? "", x.Count(), x.Sum(e => e.Minutes));
            })
            .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase).ToList();

        return new TimesheetDayResponse(date, !closed && date <= WorkshopDay.Today(clock), closed, rows,
            [.. changes.Select(x => new TimesheetChangeResponse(x.Id, x.EmployeeId, x.EmployeeName, x.BeforeMinutes,
                x.AfterMinutes, x.ChangedAt, x.ChangedByName))],
            new TimesheetMonthResponse(PayrollMonths.Format(month), byDay, byEmployee));
    }
}
