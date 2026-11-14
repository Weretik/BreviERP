namespace Reminders.Application.Features.Calendar.Commands.RemoveNonWorkingDay;

public sealed record RemoveNonWorkingDayCommand(DateOnly Date) : ICommand<Result>;
