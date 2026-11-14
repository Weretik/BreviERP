using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Tasks.Queries.GetReminderTasks;

public sealed record GetReminderTasksQuery : IQuery<Result<IReadOnlyList<ReminderTaskResponse>>>;
