using Reminders.Application.Tasks.Mapping;

namespace Reminders.Application.Tasks.CreateTask;

public sealed class CreateReminderTaskCommandValidator : AbstractValidator<CreateReminderTaskCommand>
{
    public CreateReminderTaskCommandValidator()
    {
        RuleFor(x => x.Input).NotNull().SetValidator(new ReminderTaskInputValidator());
    }
}
