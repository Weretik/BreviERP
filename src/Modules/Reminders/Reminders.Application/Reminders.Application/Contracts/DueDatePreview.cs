namespace Reminders.Application.Contracts;

public sealed record DueDatePreview(string Period, DateOnly DueDate, DateOnly RemindFrom);
