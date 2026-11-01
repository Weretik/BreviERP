using Reminders.Application.Contracts;

namespace Reminders.Application.Tasks.UpdateTask;

public sealed record UpdateReminderTaskCommand(long Id, ReminderTaskInput Input) : ICommand<Result<ReminderTaskResponse>>;
