using Production.Application.Abstractions;
using Production.Domain.Timesheet;
using Production.Application.Features.Orders.Shared.Validation;
using Production.Application.Features.Timesheet.Shared;

namespace Production.Application.Features.Timesheet.Commands.SaveTimesheetDay;

public sealed class SaveTimesheetDayValidator : AbstractValidator<SaveTimesheetDayCommand>
{
    public SaveTimesheetDayValidator()
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
