using Reminders.Application.Features.Occurrences.Shared;

namespace Reminders.Application.Features.Occurrences.Commands.CloseReminderOccurrence;

public sealed class CloseReminderOccurrenceHandler(ReminderOccurrenceStatusChanger changer)
    : ICommandHandler<CloseReminderOccurrenceCommand, Result<ReminderOccurrenceResponse>>
{
    public async ValueTask<Result<ReminderOccurrenceResponse>> Handle(CloseReminderOccurrenceCommand command,
        CancellationToken cancellationToken) => await changer.ChangeAsync(command.Id, command.Admin, true, cancellationToken);
}
