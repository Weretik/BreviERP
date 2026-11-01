using Reminders.Application.Abstractions;
using Reminders.Domain.Identifiers;

namespace Reminders.Application.Chats.SendTestMessage;

public sealed class SendReminderChatTestCommandHandler(IRemindersDbContext db, ITelegramGateway telegram)
    : ICommandHandler<SendReminderChatTestCommand, Result>
{
    public async ValueTask<Result> Handle(SendReminderChatTestCommand command, CancellationToken cancellationToken)
    {
        var chat = await db.Chats.AsNoTracking().FirstOrDefaultAsync(x => x.Id == new ReminderChatId(command.Id), cancellationToken);
        if (chat is null) return Result.NotFound();
        if (!telegram.IsConfigured) return Result.Conflict("Telegram bot token is not configured.");
        if (!chat.IsBotMember) return Result.Conflict("The bot is not a member of this chat.");
        await telegram.SendMessageAsync(chat.TelegramChatId,
            "✅ Тестове повідомлення від Brevi. Нагадування для цього чату працюють.", null, cancellationToken);
        return Result.Success();
    }
}
