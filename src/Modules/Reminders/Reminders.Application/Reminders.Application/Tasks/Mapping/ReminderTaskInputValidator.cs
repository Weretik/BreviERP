using Reminders.Application.Contracts;
using Reminders.Domain.Tasks;

namespace Reminders.Application.Tasks.Mapping;

public sealed class ReminderTaskInputValidator : AbstractValidator<ReminderTaskInput>
{
    public ReminderTaskInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(ReminderTask.TitleMaxLength);
        RuleFor(x => x.Organization).NotEmpty().Must(ReminderCodes.Organizations.ContainsKey)
            .WithMessage("Organization must be none, ppBrevi or fopSohokon.");
        RuleFor(x => x.Schedule).NotNull().SetValidator(new ReminderScheduleInputValidator());
        RuleFor(x => x.ChatId).GreaterThan(0).When(x => x.ChatId is not null);
    }
}
