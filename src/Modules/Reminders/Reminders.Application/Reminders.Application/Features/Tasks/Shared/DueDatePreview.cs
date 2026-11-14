namespace Reminders.Application.Features.Tasks.Shared;

public sealed record DueDatePreview(string Period, DateOnly DueDate, DateOnly RemindFrom);
