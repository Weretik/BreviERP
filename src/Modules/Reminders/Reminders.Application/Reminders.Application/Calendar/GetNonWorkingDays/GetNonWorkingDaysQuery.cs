using Reminders.Application.Contracts;

namespace Reminders.Application.Calendar.GetNonWorkingDays;

public sealed record GetNonWorkingDaysQuery : IQuery<Result<IReadOnlyList<NonWorkingDayResponse>>>;
