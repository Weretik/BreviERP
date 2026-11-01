using Reminders.Application.Contracts;

namespace Reminders.Application.Tasks.GetTasks;

public sealed record GetReminderTasksQuery : IQuery<Result<IReadOnlyList<ReminderTaskResponse>>>;
