namespace Reminders.Application.Tasks.ArchiveTask;

public sealed class ArchiveReminderTaskCommandValidator : AbstractValidator<ArchiveReminderTaskCommand>
{
    public ArchiveReminderTaskCommandValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
