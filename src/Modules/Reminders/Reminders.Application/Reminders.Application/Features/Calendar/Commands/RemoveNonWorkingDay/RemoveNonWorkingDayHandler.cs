using Reminders.Application.Abstractions;

namespace Reminders.Application.Features.Calendar.Commands.RemoveNonWorkingDay;

public sealed class RemoveNonWorkingDayHandler(IRemindersDbContext db) : ICommandHandler<RemoveNonWorkingDayCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveNonWorkingDayCommand command, CancellationToken cancellationToken)
    {
        var day = await db.NonWorkingDays.FirstOrDefaultAsync(x => x.Date == command.Date, cancellationToken);
        if (day is null) return Result.NotFound();
        db.NonWorkingDays.Remove(day);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
