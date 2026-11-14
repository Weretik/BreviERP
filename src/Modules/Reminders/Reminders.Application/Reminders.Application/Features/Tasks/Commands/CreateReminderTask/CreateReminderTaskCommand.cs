using Reminders.Application.Features.Tasks.Shared;

namespace Reminders.Application.Features.Tasks.Commands.CreateReminderTask;

public sealed record CreateReminderTaskCommand(ReminderTaskInput Input) : ICommand<Result<ReminderTaskResponse>>;
