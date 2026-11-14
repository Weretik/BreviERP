using Reminders.Application.Abstractions;
using Reminders.Domain.Identifiers;

namespace Reminders.Application.Features.Tasks.Commands.ArchiveReminderTask;

public sealed class ArchiveReminderTaskHandler(IRemindersDbContext db, ReminderClock clock)
    : ICommandHandler<ArchiveReminderTaskCommand, Result>
{
    public async ValueTask<Result> Handle(ArchiveReminderTaskCommand command, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == new ReminderTaskId(command.Id) && !x.IsDeleted, cancellationToken);
        if (task is null) return Result.NotFound();
        task.MarkAsDeleted(clock.Now);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
