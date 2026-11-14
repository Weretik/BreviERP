using Reminders.Application.Features.Calendar.Shared;

namespace Reminders.Application.Features.Calendar.Queries.GetNonWorkingDays;

public sealed record GetNonWorkingDaysQuery : IQuery<Result<IReadOnlyList<NonWorkingDayResponse>>>;
