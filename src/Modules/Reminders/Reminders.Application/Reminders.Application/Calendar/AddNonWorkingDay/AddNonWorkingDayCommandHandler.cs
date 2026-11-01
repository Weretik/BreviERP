using Reminders.Application.Abstractions;
using Reminders.Application.Contracts;
using Reminders.Domain.Calendar;

namespace Reminders.Application.Calendar.AddNonWorkingDay;

public sealed class AddNonWorkingDayCommandHandler(IRemindersDbContext db)
    : ICommandHandler<AddNonWorkingDayCommand, Result<NonWorkingDayResponse>>
{
    public async ValueTask<Result<NonWorkingDayResponse>> Handle(AddNonWorkingDayCommand command, CancellationToken cancellationToken)
    {
        if (await db.NonWorkingDays.AnyAsync(x => x.Date == command.Input.Date, cancellationToken))
            return Result<NonWorkingDayResponse>.Conflict("This date is already a non-working day.");
        var day = new NonWorkingDay(command.Input.Date, command.Input.Note);
        db.NonWorkingDays.Add(day);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(new NonWorkingDayResponse(day.Date, day.Note));
    }
}
