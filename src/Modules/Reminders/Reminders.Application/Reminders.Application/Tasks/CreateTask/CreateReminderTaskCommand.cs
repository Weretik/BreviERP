using Reminders.Application.Contracts;

namespace Reminders.Application.Tasks.CreateTask;

public sealed record CreateReminderTaskCommand(ReminderTaskInput Input) : ICommand<Result<ReminderTaskResponse>>;
