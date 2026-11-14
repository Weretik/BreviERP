namespace Production.Application.Features.Settings.Shared;

/// <summary>«Налаштування цеху».</summary>
/// <param name="BotConfigured">The server has the bot token.</param>
/// <param name="Chats">Groups the bot is in, to pick the workshop group from.</param>
public sealed record WorkshopSettingsResponse(
    bool TelegramEnabled, long? TelegramChatId, int? TelegramThreadId, bool BotConfigured,
    IReadOnlyList<WorkshopTelegramChatResponse> Chats, string? UpdatedByName, DateTimeOffset? UpdatedAt);
