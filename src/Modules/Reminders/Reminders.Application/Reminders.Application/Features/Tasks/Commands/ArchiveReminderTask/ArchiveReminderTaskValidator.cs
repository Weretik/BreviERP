namespace Reminders.Application.Features.Tasks.Commands.ArchiveReminderTask;

public sealed class ArchiveReminderTaskValidator : AbstractValidator<ArchiveReminderTaskCommand>
{
    public ArchiveReminderTaskValidator() => RuleFor(x => x.Id).GreaterThan(0);
}
