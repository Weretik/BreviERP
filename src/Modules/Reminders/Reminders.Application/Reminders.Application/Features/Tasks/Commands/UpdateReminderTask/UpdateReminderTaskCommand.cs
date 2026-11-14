using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Tasks.Commands.UpdateReminderTask;

public sealed record UpdateReminderTaskCommand(long Id, ReminderTaskInput Input) : ICommand<Result<ReminderTaskResponse>>;
