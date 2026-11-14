using Reminders.Application.Features.Tasks.Shared.Mapping;

namespace Reminders.Application.Features.Tasks.Commands.UpdateReminderTask;

public sealed class UpdateReminderTaskValidator : AbstractValidator<UpdateReminderTaskCommand>
{
    public UpdateReminderTaskValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new ReminderTaskInputValidator());
    }
}
