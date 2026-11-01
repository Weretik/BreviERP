using Reminders.Application.Contracts;
using Reminders.Domain.Schedules;
using Reminders.Domain.Tasks;

namespace Reminders.Application.Tasks.Mapping;

public sealed class ReminderScheduleInputValidator : AbstractValidator<ReminderScheduleInput>
{
    public ReminderScheduleInputValidator()
    {
        RuleFor(x => x.Periodicity).NotEmpty().Must(ReminderCodes.Periodicities.ContainsKey)
            .WithMessage("Періодичність: monthly, quarterly або yearly.");
        RuleFor(x => x.DueRule).NotEmpty().Must(ReminderCodes.DueRules.ContainsKey)
            .WithMessage("Правило терміну: dayOfMonth, lastWorkingDayOfMonth або daysAfterPeriodEnd.");
        RuleFor(x => x.LeadDays).InclusiveBetween(0, ReminderSchedule.MaxLeadDays);
        RuleFor(x => x).Custom((input, context) =>
        {
            if (input.DueRule is null || !ReminderCodes.DueRules.TryGetValue(input.DueRule, out var kind)) return;
            foreach (var error in DueRule.Validate(kind, input.Day, input.MonthOffset)) context.AddFailure(error);
        });
    }
}
