namespace Reminders.Application.Features.Chats.Shared;

public sealed record ReminderChatResponse(
    long Id,
    long TelegramChatId,
    string Kind,
    string TelegramTitle,
    string? DisplayName,
    bool IsApproved,
    bool IsBotMember,
    DateTimeOffset DiscoveredAt,
    bool ReceivesSiteRequests);
