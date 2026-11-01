using Reminders.Application.Contracts;

namespace Reminders.Application.Occurrences.CloseOccurrence;

public sealed class CloseReminderOccurrenceCommandHandler(ReminderOccurrenceStatusChanger changer)
    : ICommandHandler<CloseReminderOccurrenceCommand, Result<ReminderOccurrenceResponse>>
{
    public async ValueTask<Result<ReminderOccurrenceResponse>> Handle(CloseReminderOccurrenceCommand command,
        CancellationToken cancellationToken) => await changer.ChangeAsync(command.Id, command.Admin, true, cancellationToken);
}
