using Reminders.Application.Tasks.Mapping;

namespace Reminders.Application.Tasks.UpdateTask;

public sealed class UpdateReminderTaskCommandValidator : AbstractValidator<UpdateReminderTaskCommand>
{
    public UpdateReminderTaskCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new ReminderTaskInputValidator());
    }
}
