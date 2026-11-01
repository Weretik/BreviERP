namespace Reminders.Application.Occurrences.ReopenOccurrence;

public sealed class ReopenReminderOccurrenceCommandValidator : AbstractValidator<ReopenReminderOccurrenceCommand>
{
    public ReopenReminderOccurrenceCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Admin).NotNull();
    }
}
