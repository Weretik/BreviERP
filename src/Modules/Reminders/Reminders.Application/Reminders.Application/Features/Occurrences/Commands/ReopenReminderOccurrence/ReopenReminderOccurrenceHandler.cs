using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Application.Features.Occurrences.Commands.ReopenReminderOccurrence;

public sealed class ReopenReminderOccurrenceHandler(ReminderOccurrenceStatusChanger changer)
    : ICommandHandler<ReopenReminderOccurrenceCommand, Result<ReminderOccurrenceResponse>>
{
    public async ValueTask<Result<ReminderOccurrenceResponse>> Handle(ReopenReminderOccurrenceCommand command,
        CancellationToken cancellationToken) => await changer.ChangeAsync(command.Id, command.Admin, false, cancellationToken);
}
