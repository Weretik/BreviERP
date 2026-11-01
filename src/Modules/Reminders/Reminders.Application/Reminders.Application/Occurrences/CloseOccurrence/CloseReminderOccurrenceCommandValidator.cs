namespace Reminders.Application.Occurrences.CloseOccurrence;

public sealed class CloseReminderOccurrenceCommandValidator : AbstractValidator<CloseReminderOccurrenceCommand>
{
    public CloseReminderOccurrenceCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Admin).NotNull();
    }
}
