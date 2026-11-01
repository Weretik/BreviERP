using Reminders.Application.Tasks.Mapping;

namespace Reminders.Application.Tasks.PreviewDueDates;

public sealed class PreviewReminderDueDatesQueryValidator : AbstractValidator<PreviewReminderDueDatesQuery>
{
    public PreviewReminderDueDatesQueryValidator() =>
        RuleFor(x => x.Schedule).NotNull().SetValidator(new ReminderScheduleInputValidator());
}
