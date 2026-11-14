using Reminders.Domain.Tasks;
using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Tasks.Shared.Mapping;

public sealed class ReminderTaskInputValidator : AbstractValidator<ReminderTaskInput>
{
    public ReminderTaskInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(ReminderTask.TitleMaxLength);
        RuleFor(x => x.Organization).NotEmpty().Must(ReminderCodes.Organizations.ContainsKey)
            .WithMessage("Організація: none, ppBrevi або fopSohokon.");
        RuleFor(x => x.Schedule).NotNull().SetValidator(new ReminderScheduleInputValidator());
        RuleFor(x => x.ChatId).GreaterThan(0).When(x => x.ChatId is not null);
    }
}
