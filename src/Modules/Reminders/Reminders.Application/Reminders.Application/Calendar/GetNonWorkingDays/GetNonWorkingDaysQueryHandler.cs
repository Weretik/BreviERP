using Reminders.Application.Abstractions;
using Reminders.Application.Contracts;

namespace Reminders.Application.Calendar.GetNonWorkingDays;

public sealed class GetNonWorkingDaysQueryHandler(IRemindersDbContext db)
    : IQueryHandler<GetNonWorkingDaysQuery, Result<IReadOnlyList<NonWorkingDayResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<NonWorkingDayResponse>>> Handle(GetNonWorkingDaysQuery query, CancellationToken cancellationToken)
    {
        var days = await db.NonWorkingDays.AsNoTracking().OrderBy(x => x.Date).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<NonWorkingDayResponse>>(days.Select(x => new NonWorkingDayResponse(x.Date, x.Note)).ToArray());
    }
}
