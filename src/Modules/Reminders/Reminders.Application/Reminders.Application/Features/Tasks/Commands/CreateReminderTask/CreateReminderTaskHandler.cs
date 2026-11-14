using Reminders.Application.Abstractions;
using Reminders.Domain.Tasks;
using Reminders.Application.Features.Calendar.Shared;
using Reminders.Application.Features.Tasks.Shared;
using Reminders.Application.Features.Tasks.Shared.Mapping;
using BuildingBlocks.Application;

namespace Reminders.Application.Features.Tasks.Commands.CreateReminderTask;

public sealed class CreateReminderTaskHandler(IRemindersDbContext db, ReminderClock clock)
    : ICommandHandler<CreateReminderTaskCommand, Result<ReminderTaskResponse>>
{
    public async ValueTask<Result<ReminderTaskResponse>> Handle(CreateReminderTaskCommand command, CancellationToken cancellationToken)
    {
        var input = command.Input;
        var chat = await ReminderTaskChats.FindAsync(db, input.ChatId, cancellationToken);
        if (input.ChatId is not null && chat is null) return ReminderTaskChats.UnknownChat<ReminderTaskResponse>();

        var created = ReminderTask.Create(input.Title, ReminderCodes.Organizations[input.Organization],
            ReminderTaskMapper.ToSchedule(input.Schedule), chat?.Id, input.ActiveFrom ?? clock.Today, input.IsEnabled, clock.Now);
        if (created.IsFailure) return created.ToResult<ReminderTask, ReminderTaskResponse>();
        var task = created.Value;
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
        var calendar = await db.LoadCalendarAsync(cancellationToken);
        return Result.Success(ReminderTaskMapper.ToResponse(task, chat, clock.Today, calendar));
    }
}
