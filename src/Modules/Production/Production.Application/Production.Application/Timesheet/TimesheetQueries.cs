using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;
using Production.Application.Payroll;
using Production.Application.Work;
using Production.Domain.Timesheet;

namespace Production.Application.Timesheet;

internal static class TimesheetMessages
{
    public const string CannotSee = "«Робочий час» веде начальник цеху або адміністратор.";
    public const string FutureDate = "Не можна відмічати години на день, що ще не настав.";
    public const string Minutes = "Години від 0 до 24 з кроком 0,25 (15 хвилин). Для відсутності вкажіть 0.";
    public const string Stale = "Години цього працівника вже змінив хтось інший. Оновіть сторінку й внесіть зміни ще раз.";
    public const string EmployeeNotFound = "Працівника не знайдено.";
    public const string Duplicate = "Працівник указаний двічі.";
}

/// <summary>«Робочий час» of a day (PRD-D41): active workers and anyone marked that day.</summary>
public sealed record GetTimesheetDayQuery(ProductionActor Actor, DateOnly Date) : IQuery<Result<TimesheetDayResponse>>;

public sealed class GetTimesheetDayQueryValidator : AbstractValidator<GetTimesheetDayQuery>
{
    public GetTimesheetDayQueryValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Date).NotEmpty();
    }
}

public sealed class GetTimesheetDayQueryHandler(TimesheetDays days)
    : IQueryHandler<GetTimesheetDayQuery, Result<TimesheetDayResponse>>
{
    public async ValueTask<Result<TimesheetDayResponse>> Handle(GetTimesheetDayQuery query,
        CancellationToken cancellationToken) =>
        query.Actor.CanKeepTime
            ? Result.Success(await days.BuildAsync(query.Date, cancellationToken))
            : Result<TimesheetDayResponse>.Forbidden(TimesheetMessages.CannotSee);
}

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

/// <summary>«Зберегти день»: the marks of the changed workers, each change kept in the history.</summary>
public sealed record SaveTimesheetDayCommand(ProductionActor Actor, SaveTimesheetDayInput Input)
    : ICommand<Result<TimesheetDayResponse>>;

public sealed class SaveTimesheetDayCommandValidator : AbstractValidator<SaveTimesheetDayCommand>
{
    public SaveTimesheetDayCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Input).NotNull();
        When(x => x.Input is not null, () =>
        {
            RuleFor(x => x.Input.Date).NotEmpty();
            RuleFor(x => x.Input.Entries).NotNull().Must(x => x is { Count: > 0 and <= 500 })
                .WithMessage("Немає змін для збереження.");
            RuleFor(x => x.Input.Entries).Must(x => x.Select(e => e.EmployeeId).Distinct().Count() == x.Count)
                .WithMessage(TimesheetMessages.Duplicate).When(x => x.Input.Entries is not null);
            RuleForEach(x => x.Input.Entries).ChildRules(entry =>
            {
                entry.RuleFor(x => x.EmployeeId).GreaterThan(0);
                entry.RuleFor(x => x.Minutes).Must(WorkshopTimeEntry.IsValid).WithMessage(TimesheetMessages.Minutes);
            });
        });
    }
}

public sealed class SaveTimesheetDayCommandHandler(IProductionDbContext db, IProductionStaffReader staff,
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
            if (row is null) db.TimeEntries.Add(new WorkshopTimeEntry(entry.EmployeeId, input.Date, entry.Minutes, actorName, now));
            else row.Change(entry.Minutes, actorName, now);
        }

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await days.BuildAsync(input.Date, cancellationToken));
    }
}
