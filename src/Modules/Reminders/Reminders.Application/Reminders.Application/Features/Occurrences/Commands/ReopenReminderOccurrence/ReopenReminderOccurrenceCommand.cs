using Reminders.Application.Features.Access.Shared;
using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Application.Features.Occurrences.Commands.ReopenReminderOccurrence;

/// <summary>Reopens a period closed by mistake; reminders resume on the next working day run.</summary>
public sealed record ReopenReminderOccurrenceCommand(long Id, ReminderAdmin Admin) : ICommand<Result<ReminderOccurrenceResponse>>;
