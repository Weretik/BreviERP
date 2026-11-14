using Reminders.Application.Abstractions;
using Reminders.Domain.Identifiers;
using Reminders.Application.Abstractions.Telegram;

namespace Reminders.Application.Features.Chats.Commands.SendReminderChatTest;

public sealed class SendReminderChatTestHandler(IRemindersDbContext db, ITelegramGateway telegram)
    : ICommandHandler<SendReminderChatTestCommand, Result>
{
    public async ValueTask<Result> Handle(SendReminderChatTestCommand command, CancellationToken cancellationToken)
    {
        var chat = await db.Chats.AsNoTracking().FirstOrDefaultAsync(x => x.Id == new ReminderChatId(command.Id), cancellationToken);
        if (chat is null) return Result.NotFound();
        if (!telegram.IsConfigured) return Result.Conflict("Токен Telegram-бота не налаштовано.");
        if (!chat.IsBotMember) return Result.Conflict("Бота немає в цьому чаті.");
        await telegram.SendMessageAsync(chat.TelegramChatId,
            "✅ Тестове повідомлення від Brevi. Нагадування для цього чату працюють.", null, cancellationToken);
        return Result.Success();
    }
}
