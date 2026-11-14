namespace Reminders.Application.Features.Occurrences.Commands.CloseReminderOccurrence;

public sealed class CloseReminderOccurrenceValidator : AbstractValidator<CloseReminderOccurrenceCommand>
{
    public CloseReminderOccurrenceValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Admin).NotNull();
    }
}
