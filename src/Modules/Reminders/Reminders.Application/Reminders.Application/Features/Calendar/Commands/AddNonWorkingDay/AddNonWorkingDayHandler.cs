using Reminders.Application.Abstractions;
using Reminders.Domain.Calendar;
using Reminders.Application.Features.Calendar.Shared;

namespace Reminders.Application.Features.Calendar.Commands.AddNonWorkingDay;

public sealed class AddNonWorkingDayHandler(IRemindersDbContext db)
    : ICommandHandler<AddNonWorkingDayCommand, Result<NonWorkingDayResponse>>
{
    public async ValueTask<Result<NonWorkingDayResponse>> Handle(AddNonWorkingDayCommand command, CancellationToken cancellationToken)
    {
        if (await db.NonWorkingDays.AnyAsync(x => x.Date == command.Input.Date, cancellationToken))
            return Result<NonWorkingDayResponse>.Conflict("Цей день уже позначено неробочим.");
        var day = new NonWorkingDay(command.Input.Date, command.Input.Note);
        db.NonWorkingDays.Add(day);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(new NonWorkingDayResponse(day.Date, day.Note));
    }
}
