using Reminders.Application.Abstractions;
using Reminders.Application.Contracts;
using Reminders.Domain.Identifiers;

namespace Reminders.Application.Tasks.AssignChat;

public sealed class AssignReminderChatCommandHandler(IRemindersDbContext db, ReminderClock clock)
    : ICommandHandler<AssignReminderChatCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(AssignReminderChatCommand command, CancellationToken cancellationToken)
    {
        var chatId = new ReminderChatId(command.ChatId);
        if (!await db.Chats.AnyAsync(x => x.Id == chatId, cancellationToken)) return Result<int>.NotFound();
        var organizations = command.Organizations.Select(x => ReminderCodes.Organizations[x]).Distinct().ToArray();
        var tasks = await db.Tasks.Where(x => !x.IsDeleted && organizations.Contains(x.Organization)).ToListAsync(cancellationToken);
        foreach (var task in tasks)
            task.Revise(task.Title, task.Organization, task.Schedule, chatId, task.ActiveFrom, task.IsEnabled, clock.Now);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(tasks.Count);
    }
}
