namespace Reminders.Application.Tasks.ArchiveTask;

/// <summary>Removes a task from Admin and stops its reminders; its period history stays.</summary>
public sealed record ArchiveReminderTaskCommand(long Id) : ICommand<Result>;
