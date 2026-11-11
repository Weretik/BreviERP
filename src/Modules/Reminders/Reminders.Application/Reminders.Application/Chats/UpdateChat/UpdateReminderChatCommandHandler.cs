using Reminders.Application.Abstractions;
using Reminders.Application.Contracts;
using Reminders.Domain.Identifiers;

namespace Reminders.Application.Chats.UpdateChat;

public sealed class UpdateReminderChatCommandHandler(IRemindersDbContext db, ReminderClock clock)
    : ICommandHandler<UpdateReminderChatCommand, Result<ReminderChatResponse>>
{
    public async ValueTask<Result<ReminderChatResponse>> Handle(UpdateReminderChatCommand command, CancellationToken cancellationToken)
    {
        var chat = await db.Chats.FirstOrDefaultAsync(x => x.Id == new ReminderChatId(command.Id), cancellationToken);
        if (chat is null) return Result<ReminderChatResponse>.NotFound();
        chat.Configure(command.Input.DisplayName, command.Input.IsApproved, clock.Now,
            command.Input.ReceivesSiteRequests);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(ReminderChatMapper.ToResponse(chat));
    }
}
