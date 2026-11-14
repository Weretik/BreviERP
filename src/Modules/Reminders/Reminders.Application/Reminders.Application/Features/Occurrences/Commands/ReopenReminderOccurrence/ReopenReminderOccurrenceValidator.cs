namespace Reminders.Application.Features.Occurrences.Commands.ReopenReminderOccurrence;

public sealed class ReopenReminderOccurrenceValidator : AbstractValidator<ReopenReminderOccurrenceCommand>
{
    public ReopenReminderOccurrenceValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Admin).NotNull();
    }
}
