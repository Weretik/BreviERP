using Reminders.Application.Features.Tasks.Shared.Mapping;

namespace Reminders.Application.Features.Tasks.Commands.CreateReminderTask;

public sealed class CreateReminderTaskValidator : AbstractValidator<CreateReminderTaskCommand>
{
    public CreateReminderTaskValidator()
    {
        RuleFor(x => x.Input).NotNull().SetValidator(new ReminderTaskInputValidator());
    }
}
