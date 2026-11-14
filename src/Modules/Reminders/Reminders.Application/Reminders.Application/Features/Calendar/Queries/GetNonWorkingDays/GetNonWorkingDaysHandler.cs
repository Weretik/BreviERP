using Reminders.Application.Abstractions;
using Reminders.Application.Features.Calendar.Shared;

namespace Reminders.Application.Features.Calendar.Queries.GetNonWorkingDays;

public sealed class GetNonWorkingDaysHandler(IRemindersDbContext db)
    : IQueryHandler<GetNonWorkingDaysQuery, Result<IReadOnlyList<NonWorkingDayResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<NonWorkingDayResponse>>> Handle(GetNonWorkingDaysQuery query, CancellationToken cancellationToken)
    {
        var days = await db.NonWorkingDays.AsNoTracking().OrderBy(x => x.Date).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<NonWorkingDayResponse>>(days.Select(x => new NonWorkingDayResponse(x.Date, x.Note)).ToArray());
    }
}
