using Reminders.Application.Abstractions;
using Reminders.Domain.Chats;

namespace Reminders.Application.Telegram.HandleUpdate;

/// <summary>Registers groups the bot joins and private chats that press Start. Admin approves them later.</summary>
public sealed class ReminderChatTracker(IRemindersDbContext db, ITelegramGateway telegram, ReminderClock clock)
{
    private const string PendingApproval =
        "Вітаю! Я бот нагадувань Brevi. Цей чат з'явився в адмінці в розділі «Нагадування → Чати». Нагадування почнуть приходити, коли адміністратор його підтвердить.";

    public async Task MembershipChangedAsync(TelegramChatMemberUpdated update, CancellationToken cancellationToken)
    {
        var isMember = update.NewChatMember.Status is "member" or "administrator" or "creator";
        await UpsertAsync(update.Chat, isMember, cancellationToken);
    }

    public async Task MessageReceivedAsync(TelegramMessage message, CancellationToken cancellationToken)
    {
        var isStart = message.Text?.Trim().StartsWith("/start", StringComparison.OrdinalIgnoreCase) == true;
        if (!isStart) return;
        var chat = await UpsertAsync(message.Chat, true, cancellationToken);
        if (chat is { IsApproved: false })
            await telegram.SendMessageAsync(message.Chat.Id, PendingApproval, null, cancellationToken);
    }

    private async Task<ReminderChat?> UpsertAsync(TelegramChat source, bool isMember, CancellationToken cancellationToken)
    {
        ReminderChatKind? kind = source.Type switch
        {
            "group" or "supergroup" => ReminderChatKind.Group,
            "private" => ReminderChatKind.Private,
            _ => null
        };
        if (kind is null) return null;
        var chat = await db.Chats.FirstOrDefaultAsync(x => x.TelegramChatId == source.Id, cancellationToken);
        if (chat is null)
        {
            if (!isMember) return null;
            chat = ReminderChat.Discover(source.Id, kind.Value, source.DisplayTitle, clock.Now);
            db.Chats.Add(chat);
        }
        else chat.Seen(source.DisplayTitle, isMember, clock.Now);
        await db.SaveChangesAsync(cancellationToken);
        return chat;
    }
}
