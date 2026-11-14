using Reminders.Application.Features.Calendar.Shared;

namespace Reminders.Application.Features.Calendar.Commands.AddNonWorkingDay;

/// <summary>Adds an extra day off. Reminders skip it and "last working day" moves before it.</summary>
public sealed record AddNonWorkingDayCommand(NonWorkingDayInput Input) : ICommand<Result<NonWorkingDayResponse>>;
