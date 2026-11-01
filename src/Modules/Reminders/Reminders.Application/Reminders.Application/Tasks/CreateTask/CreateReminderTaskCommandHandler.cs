using Reminders.Application.Abstractions;
using Reminders.Application.Calendar;
using Reminders.Application.Contracts;
using Reminders.Application.Tasks.Mapping;
using Reminders.Domain.Tasks;

namespace Reminders.Application.Tasks.CreateTask;

public sealed class CreateReminderTaskCommandHandler(IRemindersDbContext db, ReminderClock clock)
    : ICommandHandler<CreateReminderTaskCommand, Result<ReminderTaskResponse>>
{
    public async ValueTask<Result<ReminderTaskResponse>> Handle(CreateReminderTaskCommand command, CancellationToken cancellationToken)
    {
        var input = command.Input;
        var chat = await ReminderTaskChats.FindAsync(db, input.ChatId, cancellationToken);
        if (input.ChatId is not null && chat is null) return ReminderTaskChats.UnknownChat<ReminderTaskResponse>();

        var task = ReminderTask.Create(input.Title, ReminderCodes.Organizations[input.Organization],
            ReminderTaskMapper.ToSchedule(input.Schedule), chat?.Id, input.ActiveFrom ?? clock.Today, input.IsEnabled, clock.Now);
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        var calendar = await db.LoadCalendarAsync(cancellationToken);
        return Result.Success(ReminderTaskMapper.ToResponse(task, chat, clock.Today, calendar));
    }
}
