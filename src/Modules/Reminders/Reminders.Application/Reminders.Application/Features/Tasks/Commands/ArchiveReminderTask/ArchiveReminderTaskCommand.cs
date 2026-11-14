namespace Reminders.Application.Features.Tasks.Commands.ArchiveReminderTask;

/// <summary>Removes a task from Admin and stops its reminders; its period history stays.</summary>
public sealed record ArchiveReminderTaskCommand(long Id) : ICommand<Result>;
