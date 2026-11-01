using Reminders.Application.Abstractions;
using Reminders.Domain.Identifiers;

namespace Reminders.Application.Tasks.Mapping;

internal static class ReminderTaskChats
{
    public static async Task<Domain.Chats.ReminderChat?> FindAsync(IRemindersDbContext db, long? chatId, CancellationToken cancellationToken) =>
        chatId is null ? null : await db.Chats.FirstOrDefaultAsync(x => x.Id == new ReminderChatId(chatId.Value), cancellationToken);

    public static Result<T> UnknownChat<T>() =>
        Result<T>.Invalid(new ValidationError("Input.ChatId", "Chat was not found.", "unknown_chat", ValidationSeverity.Error));
}
