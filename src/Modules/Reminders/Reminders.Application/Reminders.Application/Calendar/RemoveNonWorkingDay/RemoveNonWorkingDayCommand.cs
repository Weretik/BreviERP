namespace Reminders.Application.Calendar.RemoveNonWorkingDay;

public sealed record RemoveNonWorkingDayCommand(DateOnly Date) : ICommand<Result>;
