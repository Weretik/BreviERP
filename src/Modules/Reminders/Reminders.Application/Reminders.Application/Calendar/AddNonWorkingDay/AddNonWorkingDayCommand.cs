using Reminders.Application.Contracts;

namespace Reminders.Application.Calendar.AddNonWorkingDay;

/// <summary>Adds an extra day off. Reminders skip it and "last working day" moves before it.</summary>
public sealed record AddNonWorkingDayCommand(NonWorkingDayInput Input) : ICommand<Result<NonWorkingDayResponse>>;
