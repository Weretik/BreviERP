namespace Reminders.Application.Contracts;

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
