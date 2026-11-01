using Reminders.Application.Access;
using Reminders.Application.Contracts;

namespace Reminders.Application.Occurrences.CloseOccurrence;

/// <summary>Marks a period done from Admin, for example when the button was not pressed in Telegram.</summary>
public sealed record CloseReminderOccurrenceCommand(long Id, ReminderAdmin Admin) : ICommand<Result<ReminderOccurrenceResponse>>;
