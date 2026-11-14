using Reminders.Application.Features.Tasks.Shared.Mapping;

namespace Reminders.Application.Features.Tasks.Queries.PreviewReminderDueDates;

public sealed class PreviewReminderDueDatesValidator : AbstractValidator<PreviewReminderDueDatesQuery>
{
    public PreviewReminderDueDatesValidator() =>
        RuleFor(x => x.Schedule).NotNull().SetValidator(new ReminderScheduleInputValidator());
}
