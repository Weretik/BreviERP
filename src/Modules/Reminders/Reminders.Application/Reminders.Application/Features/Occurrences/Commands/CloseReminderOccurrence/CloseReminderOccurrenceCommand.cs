using Reminders.Application.Features.Access.Shared;
using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Application.Features.Occurrences.Commands.CloseReminderOccurrence;

/// <summary>Marks a period done from Admin, for example when the button was not pressed in Telegram.</summary>
public sealed record CloseReminderOccurrenceCommand(long Id, ReminderAdmin Admin) : ICommand<Result<ReminderOccurrenceResponse>>;
