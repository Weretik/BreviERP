using Reminders.Application.Abstractions;
using Reminders.Domain.Identifiers;
using Reminders.Domain.Occurrences;
using Reminders.Application.Features.Calendar.Shared;
using Reminders.Application.Features.Tasks.Shared;
using Reminders.Application.Features.Tasks.Shared.Mapping;
using BuildingBlocks.Application;

namespace Reminders.Application.Features.Tasks.Commands.UpdateReminderTask;

public sealed class UpdateReminderTaskHandler(IRemindersDbContext db, ReminderClock clock)
    : ICommandHandler<UpdateReminderTaskCommand, Result<ReminderTaskResponse>>
{
    public async ValueTask<Result<ReminderTaskResponse>> Handle(UpdateReminderTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == new ReminderTaskId(command.Id) && !x.IsDeleted, cancellationToken);
        if (task is null) return Result<ReminderTaskResponse>.NotFound();
        var input = command.Input;
        var chat = await ReminderTaskChats.FindAsync(db, input.ChatId, cancellationToken);
        if (input.ChatId is not null && chat is null) return ReminderTaskChats.UnknownChat<ReminderTaskResponse>();

        var revised = task.Revise(input.Title, ReminderCodes.Organizations[input.Organization],
            ReminderTaskMapper.ToSchedule(input.Schedule), chat?.Id, input.ActiveFrom ?? task.ActiveFrom, input.IsEnabled, clock.Now);
        if (revised.IsFailure) return revised.ToResult<ReminderTaskResponse>();
        var calendar = await db.LoadCalendarAsync(cancellationToken);
        var open = await db.Occurrences.Where(x => x.TaskId == task.Id && x.Status == ReminderOccurrenceStatus.Open).ToListAsync(cancellationToken);
        foreach (var occurrence in open) occurrence.Reschedule(task.Schedule.DueDate(occurrence.Period, calendar));
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(ReminderTaskMapper.ToResponse(task, chat, clock.Today, calendar));
    }
}
