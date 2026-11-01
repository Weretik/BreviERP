using Reminders.Application.Contracts;

namespace Reminders.Application.Occurrences.ReopenOccurrence;

public sealed class ReopenReminderOccurrenceCommandHandler(ReminderOccurrenceStatusChanger changer)
    : ICommandHandler<ReopenReminderOccurrenceCommand, Result<ReminderOccurrenceResponse>>
{
    public async ValueTask<Result<ReminderOccurrenceResponse>> Handle(ReopenReminderOccurrenceCommand command,
        CancellationToken cancellationToken) => await changer.ChangeAsync(command.Id, command.Admin, false, cancellationToken);
}
