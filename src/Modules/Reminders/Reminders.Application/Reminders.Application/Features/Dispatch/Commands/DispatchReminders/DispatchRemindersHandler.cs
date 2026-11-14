using Reminders.Application.Features.Dispatch.Shared;

namespace Reminders.Application.Features.Dispatch.Commands.DispatchReminders;

public sealed class DispatchRemindersHandler(ReminderDispatcher dispatcher)
    : ICommandHandler<DispatchRemindersCommand, Result<ReminderDispatchSummary>>
{
    public async ValueTask<Result<ReminderDispatchSummary>> Handle(DispatchRemindersCommand command,
        CancellationToken cancellationToken) =>
        Result.Success(await dispatcher.RunAsync(cancellationToken));
}
